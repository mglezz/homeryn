using System;
using System.ComponentModel;
using System.Net.Sockets;
using System.Text.Json;
using Homeryn.Desktop.Networking;
using Homeryn.Shared.Chores;

namespace Homeryn.Desktop.Forms
{
    public partial class ChoresForm : Form
    {
        private readonly ChoresClient client = new ChoresClient();
        private readonly CancellationTokenSource lifetime = new CancellationTokenSource();
        private readonly BindingList<Chore> items = new BindingList<Chore>();
        private bool isBusy;
        private bool requiresRefresh = true;
        private bool isUpdatingSelection;

        public ChoresForm()
        {
            InitializeComponent();
            ConfigureItemsGrid();
            UpdateButtons();
        }

        protected override async void OnShown(EventArgs e)
        {
            base.OnShown(e);
            await RunOperationAsync(RefreshItemsAsync);
        }

        private void ConfigureItemsGrid()
        {
            itemsGrid.AutoGenerateColumns = false;
            itemsGrid.AllowUserToAddRows = false;
            itemsGrid.AllowUserToDeleteRows = false;
            itemsGrid.ReadOnly = true;
            itemsGrid.MultiSelect = false;
            itemsGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            itemsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            itemsGrid.RowHeadersVisible = false;

            DataGridViewTextBoxColumn nameColumn = new DataGridViewTextBoxColumn();
            nameColumn.DataPropertyName = nameof(Chore.Name);
            nameColumn.HeaderText = "Chore";
            nameColumn.FillWeight = 40;
            DataGridViewTextBoxColumn assigneeColumn = new DataGridViewTextBoxColumn();
            assigneeColumn.DataPropertyName = nameof(Chore.Assignee);
            assigneeColumn.HeaderText = "Responsible person";
            assigneeColumn.DefaultCellStyle.NullValue = "Unassigned";
            assigneeColumn.FillWeight = 25;
            DataGridViewTextBoxColumn dueDateColumn = new DataGridViewTextBoxColumn();
            dueDateColumn.DataPropertyName = nameof(Chore.DueDate);
            dueDateColumn.HeaderText = "Due date";
            dueDateColumn.DefaultCellStyle.Format = "d";
            dueDateColumn.DefaultCellStyle.NullValue = "No date";
            dueDateColumn.FillWeight = 20;
            DataGridViewCheckBoxColumn completedColumn = new DataGridViewCheckBoxColumn();
            completedColumn.DataPropertyName = nameof(Chore.IsCompleted);
            completedColumn.HeaderText = "Completed";
            completedColumn.FillWeight = 15;
            itemsGrid.Columns.AddRange(nameColumn, assigneeColumn, dueDateColumn, completedColumn);
            itemsGrid.DataSource = items;
        }

        private Chore? GetSelectedItem()
        {
            if (itemsGrid.SelectedRows.Count == 0)
            {
                return null;
            }
            return itemsGrid.SelectedRows[0].DataBoundItem as Chore;
        }

        private ChoreInput ReadInput()
        {
            DateOnly? dueDate = null;
            if (dueDatePicker.Checked)
            {
                dueDate = DateOnly.FromDateTime(dueDatePicker.Value);
            }
            return new ChoreInput(itemNameTextBox.Text, assigneeTextBox.Text, dueDate, completedCheckBox.Checked);
        }

        private void ItemsGrid_SelectionChanged(object? sender, EventArgs e)
        {
            if (isUpdatingSelection || isBusy)
            {
                return;
            }
            Chore? item = GetSelectedItem();
            if (item != null)
            {
                itemNameTextBox.Text = item.Name;
                assigneeTextBox.Text = item.Assignee ?? "";
                dueDatePicker.Value = item.DueDate?.ToDateTime(TimeOnly.MinValue) ?? DateTime.Today;
                dueDatePicker.Checked = item.DueDate.HasValue;
                completedCheckBox.Checked = item.IsCompleted;
            }
            UpdateButtons();
        }

        private void ItemNameTextBox_TextChanged(object? sender, EventArgs e)
        {
            UpdateButtons();
        }

        private async void AddButton_Click(object? sender, EventArgs e)
        {
            if (requiresRefresh || GetSelectedItem() != null)
            {
                return;
            }
            await RunOperationAsync(async () =>
            {
                Chore item = await client.AddAsync(ReadInput(), lifetime.Token);
                if (IsDisposed)
                {
                    return;
                }
                isUpdatingSelection = true;
                items.Add(item);
                ResetEditor();
                ShowListStatus("Chore added.");
            });
        }

        private async void SaveButton_Click(object? sender, EventArgs e)
        {
            Chore? selected = GetSelectedItem();
            if (selected == null || requiresRefresh)
            {
                return;
            }
            await RunOperationAsync(async () =>
            {
                Chore updated = await client.UpdateAsync(selected, ReadInput(), lifetime.Token);
                if (IsDisposed)
                {
                    return;
                }
                isUpdatingSelection = true;
                items[items.IndexOf(selected)] = updated;
                ResetEditor();
                ShowListStatus("Changes saved.");
            });
        }

        private async void RemoveButton_Click(object? sender, EventArgs e)
        {
            Chore? selected = GetSelectedItem();
            if (selected == null || isBusy || requiresRefresh)
            {
                return;
            }
            if (MessageBox.Show(this, $"Remove \"{selected.Name}\" from the list?", "Chores",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }
            await RunOperationAsync(async () =>
            {
                await client.DeleteAsync(selected, lifetime.Token);
                if (IsDisposed)
                {
                    return;
                }
                isUpdatingSelection = true;
                items.Remove(selected);
                ResetEditor();
                ShowListStatus("Chore removed.");
            });
        }

        private void NewButton_Click(object? sender, EventArgs e)
        {
            if (isBusy)
            {
                return;
            }
            isUpdatingSelection = true;
            ResetEditor();
            UpdateButtons();
            itemNameTextBox.Focus();
        }

        private async void RefreshButton_Click(object? sender, EventArgs e)
        {
            await RunOperationAsync(RefreshItemsAsync);
        }

        private async Task RefreshItemsAsync()
        {
            List<Chore> latest = await client.ReadAllAsync(lifetime.Token);
            if (IsDisposed)
            {
                return;
            }
            isUpdatingSelection = true;
            items.RaiseListChangedEvents = false;
            items.Clear();
            foreach (Chore item in latest)
            {
                items.Add(item);
            }
            items.RaiseListChangedEvents = true;
            items.ResetBindings();
            requiresRefresh = false;
            ResetEditor();
            ShowListStatus("List refreshed.");
        }

        private void ResetEditor()
        {
            itemsGrid.ClearSelection();
            itemsGrid.CurrentCell = null;
            itemNameTextBox.Clear();
            assigneeTextBox.Clear();
            dueDatePicker.Value = DateTime.Today;
            dueDatePicker.Checked = false;
            completedCheckBox.Checked = false;
            isUpdatingSelection = false;
        }

        private async Task RunOperationAsync(Func<Task> operation)
        {
            if (isBusy)
            {
                return;
            }
            isBusy = true;
            statusLabel.Text = "Contacting the server...";
            UpdateButtons();
            try
            {
                await operation();
            }
            catch (ServerRequestException exception)
            {
                if (!IsDisposed)
                {
                    ShowServerError(exception.Code);
                }
            }
            catch (OperationCanceledException)
            {
                if (!IsDisposed && !lifetime.IsCancellationRequested)
                {
                    RequireRefresh("The server took too long to respond.");
                }
            }
            catch (Exception exception) when (exception is SocketException || exception is IOException
                || exception is JsonException || exception is InvalidDataException)
            {
                if (!IsDisposed)
                {
                    RequireRefresh("The result could not be confirmed. Check that the server is running.");
                }
            }
            finally
            {
                isBusy = false;
                isUpdatingSelection = false;
                if (!IsDisposed)
                {
                    UpdateButtons();
                }
            }
        }

        private void ShowServerError(string code)
        {
            switch (code)
            {
                case "INVALID_NAME":
                    statusLabel.Text = "Enter a chore name with 1 to 80 characters and no line breaks.";
                    break;
                case "INVALID_ASSIGNEE":
                    statusLabel.Text = "Enter a responsible person with up to 60 characters and no line breaks.";
                    break;
                case "INVALID_DUE_DATE":
                    statusLabel.Text = "Choose a date from 1753 to 9998, or uncheck the date to leave it empty.";
                    break;
                case "CHORE_CONFLICT":
                    RequireRefresh("Someone changed this chore after you loaded it.");
                    break;
                case "CHORE_NOT_FOUND":
                    RequireRefresh("This chore has already been removed.");
                    break;
                case "UNKNOWN_COMMAND":
                    RequireRefresh("This server does not support chores. Restart it with the latest build.");
                    break;
                default:
                    RequireRefresh("The server could not complete this request.");
                    break;
            }
        }

        private void RequireRefresh(string message)
        {
            requiresRefresh = true;
            statusLabel.Text = message + " Refresh the list before making another change.";
        }

        private void ShowListStatus(string message)
        {
            int completed = items.Count(item => item.IsCompleted);
            statusLabel.Text = $"{message} {items.Count - completed} pending, {completed} completed. Refresh loads other changes.";
        }

        private void UpdateButtons()
        {
            bool canChange = !isBusy && !requiresRefresh;
            bool hasName = !string.IsNullOrWhiteSpace(itemNameTextBox.Text);
            bool hasSelection = GetSelectedItem() != null;
            addButton.Enabled = canChange && hasName && !hasSelection;
            saveButton.Enabled = canChange && hasName && hasSelection;
            removeButton.Enabled = canChange && hasSelection;
            refreshButton.Enabled = !isBusy;
            itemsGrid.Enabled = !isBusy;
            itemNameTextBox.Enabled = !isBusy;
            assigneeTextBox.Enabled = !isBusy;
            dueDatePicker.Enabled = !isBusy;
            newButton.Enabled = !isBusy;
            completedCheckBox.Enabled = !isBusy;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            lifetime.Cancel();
            base.OnFormClosed(e);
        }

        private void DisposeRequests()
        {
            lifetime.Cancel();
            lifetime.Dispose();
        }
    }
}
