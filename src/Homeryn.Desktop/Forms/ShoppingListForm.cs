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
        private readonly ShoppingListClient client = new ShoppingListClient();
        private readonly CancellationTokenSource lifetime = new CancellationTokenSource();
        private readonly BindingList<ShoppingItem> items = new BindingList<ShoppingItem>();
        private bool isBusy;
        private bool requiresRefresh = true;
        private bool isUpdatingSelection;

        public ShoppingListForm()
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
                itemNameTextBox.Text = item.Name;
                quantityInput.Value = item.Quantity;
                purchasedCheckBox.Checked = item.IsPurchased;
            }
            UpdateButtons();
        }

        private void ItemNameTextBox_TextChanged(object? sender, EventArgs e)
        {
            UpdateButtons();
        }

        private async void AddButton_Click(object? sender, EventArgs e)
        {
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
            if (selected == null)
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
            await RunOperationAsync(RefreshItemsAsync);
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
            bool canChange = !isBusy && !requiresRefresh;
            bool hasName = !string.IsNullOrWhiteSpace(itemNameTextBox.Text);
            bool hasSelection = GetSelectedItem() != null;
            addButton.Enabled = canChange && hasName;
            saveButton.Enabled = canChange && hasName && hasSelection;
            removeButton.Enabled = canChange && hasSelection;
            refreshButton.Enabled = !isBusy;
            itemsGrid.Enabled = !isBusy;
            itemNameTextBox.Enabled = !isBusy;
            quantityInput.Enabled = !isBusy;
            purchasedCheckBox.Enabled = !isBusy;
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
