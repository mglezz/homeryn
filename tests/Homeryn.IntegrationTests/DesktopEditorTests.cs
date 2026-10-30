using Homeryn.Desktop.Forms;
using Homeryn.Desktop.Networking;

namespace Homeryn.IntegrationTests
{
    internal static class DesktopEditorTests
    {
        public static async Task<int> RunAsync(string directory, CancellationToken token)
        {
            await using TestServer server = new TestServer(Path.Combine(directory, "editors.db"));
            foreach (bool chores in new[] { false, true })
            {
                TaskCompletionSource result = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                Thread thread = new Thread(() =>
                {
                    try
                    {
                        using Form form = CreateForm(chores, server.Port);
                        form.ShowInTaskbar = false;
                        form.Opacity = 0;
                        form.Shown += async (sender, args) =>
                        {
                            try
                            {
                                await VerifyEditorAsync(form, chores, server.Port, token);
                                form.Close();
                                result.TrySetResult();
                            }
                            catch (Exception exception)
                            {
                                // Dispose directly on failure so a dirty-editor prompt cannot block CI.
                                form.Dispose();
                                result.TrySetException(exception);
                            }
                        };
                        Application.Run(form);
                    }
                    catch (Exception exception)
                    {
                        result.TrySetException(exception);
                    }
                });
                thread.IsBackground = true;
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                await result.Task.WaitAsync(TimeSpan.FromSeconds(15), token);
                Console.WriteLine($"PASS: {(chores ? "Chore" : "Shopping")} editor preserves drafts, resets selection and recovers from another client's edit");
            }
            return 2;
        }

        private static Form CreateForm(bool chores, int port)
        {
            if (chores)
            {
                return new ChoresForm(new ChoresClient(port: port));
            }
            return new ShoppingListForm(new ShoppingListClient(port: port));
        }

        private static async Task VerifyEditorAsync(Form form, bool chores, int port, CancellationToken token)
        {
            TextBox name = Find<TextBox>(form, "itemNameTextBox");
            DataGridView grid = Find<DataGridView>(form, "itemsGrid");
            Button add = Find<Button>(form, "addButton");
            Button save = Find<Button>(form, "saveButton");
            Button reset = Find<Button>(form, "newButton");
            Button refresh = Find<Button>(form, "refreshButton");
            Label status = Find<Label>(form, "statusLabel");
            await WaitUntilAsync(() => status.Text.StartsWith("List refreshed.") && refresh.Enabled, token);
            Assert(!add.Enabled && !save.Enabled, "An empty editor must not mutate storage.");

            name.Text = "Editor test";
            Assert(add.Enabled && !save.Enabled && !grid.Enabled, "A new draft must disable row changes.");
            add.PerformClick();
            await WaitUntilAsync(() => grid.Rows.Count == 1 && refresh.Enabled, token);
            AssertReset(name, grid, add, save);

            grid.CurrentCell = grid.Rows[0].Cells[0];
            grid.Rows[0].Selected = true;
            Assert(name.Text == "Editor test" && !add.Enabled && !save.Enabled,
                "Selecting a row must load it without enabling Add or an unchanged Save.");
            if (chores)
            {
                Find<CheckBox>(form, "completedCheckBox").Checked = true;
            }
            else
            {
                Find<NumericUpDown>(form, "quantityInput").Value = 3;
            }
            Assert(save.Enabled && !add.Enabled && !grid.Enabled, "Changing a field must enable Save and preserve selection.");
            save.PerformClick();
            await WaitUntilAsync(() => status.Text.StartsWith("Changes saved.") && refresh.Enabled, token);
            AssertReset(name, grid, add, save);

            refresh.PerformClick();
            await WaitUntilAsync(() => status.Text.StartsWith("List refreshed.") && refresh.Enabled, token);
            AssertReset(name, grid, add, save);
            grid.CurrentCell = grid.Rows[0].Cells[0];
            grid.Rows[0].Selected = true;
            if (chores)
            {
                Assert(Find<CheckBox>(form, "completedCheckBox").Checked, "Completion did not persist.");
                Find<TextBox>(form, "assigneeTextBox").Text = "Test person";
                Assert(save.Enabled && !grid.Enabled, "Assignment edits must be tracked.");
                Find<TextBox>(form, "assigneeTextBox").Clear();
            }
            else
            {
                Assert(Find<NumericUpDown>(form, "quantityInput").Value == 3, "Quantity did not persist.");
                Find<CheckBox>(form, "purchasedCheckBox").Checked = true;
                Assert(save.Enabled && !grid.Enabled, "Purchase edits must be tracked.");
                Find<CheckBox>(form, "purchasedCheckBox").Checked = false;
            }
            Assert(!save.Enabled && grid.Enabled, "Reverting an edit must restore clean selection.");
            reset.PerformClick();
            AssertReset(name, grid, add, save);

            grid.CurrentCell = grid.Rows[0].Cells[0];
            grid.Rows[0].Selected = true;
            if (chores)
            {
                ChoresClient other = new ChoresClient(port: port);
                Homeryn.Shared.Chores.Chore item = (await other.ReadAllAsync(token)).Single();
                await other.UpdateAsync(item, new Homeryn.Shared.Chores.ChoreInput("Another window", item.Assignee, item.DueDate, item.IsCompleted), token);
            }
            else
            {
                ShoppingListClient other = new ShoppingListClient(port: port);
                Homeryn.Shared.Shopping.ShoppingItem item = (await other.ReadAllAsync(token)).Single();
                await other.UpdateAsync(item, new Homeryn.Shared.Shopping.ShoppingItemInput("Another window", item.Quantity, item.IsPurchased), token);
            }
            name.Text = "Unsaved local change";
            save.PerformClick();
            await WaitUntilAsync(() => status.Text.StartsWith("Someone changed") && refresh.Enabled, token);
            Assert(name.Text == "Unsaved local change" && !save.Enabled && !add.Enabled,
                "A stale edit must preserve the draft and require refresh.");
            // Revert the draft to avoid a modal confirmation in the unattended runner.
            name.Text = "Editor test";
            refresh.PerformClick();
            await WaitUntilAsync(() => status.Text.StartsWith("List refreshed.") && refresh.Enabled, token);
            AssertReset(name, grid, add, save);
            grid.CurrentCell = grid.Rows[0].Cells[0];
            grid.Rows[0].Selected = true;
            Assert(name.Text == "Another window", "Refresh did not load the other client's committed version.");
            reset.PerformClick();
            AssertReset(name, grid, add, save);
        }

        private static void AssertReset(TextBox name, DataGridView grid, Button add, Button save)
        {
            Assert(name.Text == "" && grid.Enabled && grid.SelectedRows.Count == 0 && grid.CurrentCell == null
                && !add.Enabled && !save.Enabled, "A successful operation must leave a clean editor and no automatic row selection.");
        }

        private static T Find<T>(Form form, string name) where T : Control
        {
            return (T)form.Controls.Find(name, true).Single();
        }

        private static async Task WaitUntilAsync(Func<bool> condition, CancellationToken token)
        {
            using CancellationTokenSource timeout = CancellationTokenSource.CreateLinkedTokenSource(token);
            timeout.CancelAfter(TimeSpan.FromSeconds(7));
            while (!condition())
            {
                await Task.Delay(20, timeout.Token);
            }
        }

        private static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}
