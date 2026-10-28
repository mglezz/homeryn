using System;
using System.ComponentModel;
using System.Net.Sockets;
using System.Text.Json;
using Homeryn.Desktop.Networking;
using Homeryn.Shared.Shopping;

namespace Homeryn.Desktop.Forms
{
    public partial class ShoppingListForm : Form
    {
        private readonly ShoppingListClient client;
        private readonly CancellationTokenSource lifetime = new CancellationTokenSource();
        private readonly BindingList<ShoppingItem> items = new BindingList<ShoppingItem>();
        private bool isBusy;
        private bool requiresRefresh = true;
        private bool isUpdatingSelection;

        public ShoppingListForm() : this(new ShoppingListClient())
        {
        }

        public ShoppingListForm(ShoppingListClient client)
        {
            this.client = client;
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
            nameColumn.DataPropertyName = nameof(ShoppingItem.Name);
            nameColumn.HeaderText = "Item";
            nameColumn.FillWeight = 65;
            DataGridViewTextBoxColumn quantityColumn = new DataGridViewTextBoxColumn();
            quantityColumn.DataPropertyName = nameof(ShoppingItem.Quantity);
            quantityColumn.HeaderText = "Quantity";
            quantityColumn.FillWeight = 15;
            DataGridViewCheckBoxColumn purchasedColumn = new DataGridViewCheckBoxColumn();
            purchasedColumn.DataPropertyName = nameof(ShoppingItem.IsPurchased);
            purchasedColumn.HeaderText = "Purchased";
            purchasedColumn.FillWeight = 20;
            itemsGrid.Columns.AddRange(nameColumn, quantityColumn, purchasedColumn);
            itemsGrid.DataSource = items;
        }

        private ShoppingItem? GetSelectedItem()
        {
            if (itemsGrid.SelectedRows.Count == 0)
            {
                return null;
            }
            return itemsGrid.SelectedRows[0].DataBoundItem as ShoppingItem;
        }

        private ShoppingItemInput ReadInput()
        {
            return new ShoppingItemInput(itemNameTextBox.Text, (int)quantityInput.Value, purchasedCheckBox.Checked);
        }

        private void ItemsGrid_SelectionChanged(object? sender, EventArgs e)
        {
            if (isUpdatingSelection || isBusy)
            {
                return;
            }
            ShoppingItem? item = GetSelectedItem();
            if (item != null)
            {
                isUpdatingSelection = true;
                itemNameTextBox.Text = item.Name;
                quantityInput.Value = item.Quantity;
                purchasedCheckBox.Checked = item.IsPurchased;
                isUpdatingSelection = false;
            }
            UpdateButtons();
        }

        private void Editor_ValueChanged(object? sender, EventArgs e)
        {
            UpdateButtons();
            if (!isBusy && !isUpdatingSelection && !requiresRefresh)
            {
                if (HasUnsavedChanges())
                {
                    statusLabel.Text = "Unsaved changes. Use Add or Save to keep them, or New to discard them.";
                }
                else
                {
                    ShowListStatus("No unsaved changes.");
                }
            }
        }

        private async void AddButton_Click(object? sender, EventArgs e)
        {
            if (requiresRefresh || GetSelectedItem() != null)
            {
                return;
            }
            await RunOperationAsync(async () =>
            {
                ShoppingItem item = await client.AddAsync(ReadInput(), lifetime.Token);
                if (IsDisposed)
                {
                    return;
                }
                isUpdatingSelection = true;
                items.Add(item);
                ResetEditor();
                ShowListStatus("Item added.");
            });
        }

        private async void SaveButton_Click(object? sender, EventArgs e)
        {
            ShoppingItem? selected = GetSelectedItem();
            if (selected == null || requiresRefresh)
            {
                return;
            }
            await RunOperationAsync(async () =>
            {
                ShoppingItem updated = await client.UpdateAsync(selected, ReadInput(), lifetime.Token);
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
            ShoppingItem? selected = GetSelectedItem();
            if (selected == null || isBusy || requiresRefresh)
            {
                return;
            }
            if (MessageBox.Show(this, $"Remove \"{selected.Name}\" from the list?", "Shopping list",
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
                ShowListStatus("Item removed.");
            });
        }

        private async void RefreshButton_Click(object? sender, EventArgs e)
        {
            if (isBusy || !ConfirmDiscardChanges())
            {
                return;
            }
            await RunOperationAsync(RefreshItemsAsync);
        }

        private void NewButton_Click(object? sender, EventArgs e)
        {
            if (isBusy || !ConfirmDiscardChanges())
            {
                return;
            }
            isUpdatingSelection = true;
            ResetEditor();
            UpdateButtons();
            if (requiresRefresh)
            {
                RequireRefresh("The list is not up to date.");
            }
            else
            {
                ShowListStatus("New item.");
            }
            itemNameTextBox.Focus();
        }

        private bool HasUnsavedChanges()
        {
            ShoppingItem? selected = GetSelectedItem();
            return itemNameTextBox.Text != (selected?.Name ?? "")
                || quantityInput.Value != (selected?.Quantity ?? 1)
                || purchasedCheckBox.Checked != (selected?.IsPurchased ?? false);
        }

        private bool ConfirmDiscardChanges()
        {
            return !HasUnsavedChanges() || MessageBox.Show(this,
                "Discard the unsaved changes in the editor?", "Shopping list",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes;
        }

        private async Task RefreshItemsAsync()
        {
            List<ShoppingItem> latest = await client.ReadAllAsync(lifetime.Token);
            if (IsDisposed)
            {
                return;
            }
            isUpdatingSelection = true;
            items.RaiseListChangedEvents = false;
            items.Clear();
            foreach (ShoppingItem item in latest)
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
            quantityInput.Value = 1;
            purchasedCheckBox.Checked = false;
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
                    statusLabel.Text = "Enter an item name with 1 to 80 characters and no line breaks.";
                    break;
                case "INVALID_QUANTITY":
                    statusLabel.Text = "Enter a quantity between 1 and 999.";
                    break;
                case "ITEM_CONFLICT":
                    RequireRefresh("Someone changed this item after you loaded it.");
                    break;
                case "ITEM_NOT_FOUND":
                    RequireRefresh("This item has already been removed.");
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
            statusLabel.Text = $"{message} {items.Count} item(s). Select a row to edit it; Refresh loads other changes.";
        }

        private void UpdateButtons()
        {
            if (isUpdatingSelection)
            {
                return;
            }
            bool canChange = !isBusy && !requiresRefresh;
            bool hasName = !string.IsNullOrWhiteSpace(itemNameTextBox.Text);
            bool hasSelection = GetSelectedItem() != null;
            bool hasChanges = HasUnsavedChanges();
            addButton.Enabled = canChange && hasName && !hasSelection;
            saveButton.Enabled = canChange && hasName && hasSelection && hasChanges;
            removeButton.Enabled = canChange && hasSelection;
            refreshButton.Enabled = !isBusy;
            newButton.Enabled = !isBusy;
            // Keep the selected record stable while its editor contains a draft.
            isUpdatingSelection = true;
            itemsGrid.Enabled = !isBusy && !hasChanges;
            if (!hasSelection)
            {
                // Enabling a bound grid can select its first row automatically.
                itemsGrid.ClearSelection();
                itemsGrid.CurrentCell = null;
            }
            isUpdatingSelection = false;
            itemNameTextBox.Enabled = !isBusy;
            quantityInput.Enabled = !isBusy;
            purchasedCheckBox.Enabled = !isBusy;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && !isBusy && !ConfirmDiscardChanges())
            {
                e.Cancel = true;
            }
            base.OnFormClosing(e);
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
