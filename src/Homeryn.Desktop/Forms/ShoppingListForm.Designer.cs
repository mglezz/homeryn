namespace Homeryn.Desktop.Forms
{
    partial class ShoppingListForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && !IsDisposed)
            {
                DisposeRequests();
            }
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            titleLabel = new Label();
            hintLabel = new Label();
            itemsGrid = new DataGridView();
            itemNameLabel = new Label();
            quantityLabel = new Label();
            quantityInput = new NumericUpDown();
            purchasedCheckBox = new CheckBox();
            addButton = new Button();
            removeButton = new Button();
            saveButton = new Button();
            refreshButton = new Button();
            statusLabel = new Label();
            itemNameTextBox = new TextBox();
            ((System.ComponentModel.ISupportInitialize)itemsGrid).BeginInit();
            ((System.ComponentModel.ISupportInitialize)quantityInput).BeginInit();
            SuspendLayout();
            // titleLabel
            titleLabel.AutoSize = true;
            titleLabel.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            titleLabel.Location = new Point(24, 18);
            titleLabel.Name = "titleLabel";
            titleLabel.Text = "Shopping list";
            // hintLabel
            hintLabel.AutoSize = true;
            hintLabel.Location = new Point(25, 56);
            hintLabel.Name = "hintLabel";
            hintLabel.Text = "Add an item to the list. Select a row to edit its details or mark it as purchased.";
            //
            // itemsGrid
            //
            itemsGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            itemsGrid.BackgroundColor = SystemColors.Window;
            itemsGrid.AccessibleName = "Shopping list";
            itemsGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            itemsGrid.Location = new Point(24, 85);
            itemsGrid.Name = "itemsGrid";
            itemsGrid.Size = new Size(752, 235);
            itemsGrid.TabIndex = 0;
            itemsGrid.SelectionChanged += ItemsGrid_SelectionChanged;
            //
            // itemNameLabel
            //
            itemNameLabel.AutoSize = true;
            itemNameLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            itemNameLabel.Location = new Point(24, 338);
            itemNameLabel.Name = "itemNameLabel";
            itemNameLabel.Size = new Size(64, 15);
            itemNameLabel.TabIndex = 1;
            itemNameLabel.Text = "&Item name";
            //
            // quantityLabel
            //
            quantityLabel.AutoSize = true;
            quantityLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            quantityLabel.Location = new Point(24, 402);
            quantityLabel.Name = "quantityLabel";
            quantityLabel.Size = new Size(53, 15);
            quantityLabel.TabIndex = 3;
            quantityLabel.Text = "&Quantity";
            //
            // quantityInput
            //
            quantityInput.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            quantityInput.Location = new Point(24, 426);
            quantityInput.Maximum = new decimal(new int[] { 999, 0, 0, 0 });
            quantityInput.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            quantityInput.AccessibleName = "Quantity";
            quantityInput.Name = "quantityInput";
            quantityInput.Size = new Size(200, 23);
            quantityInput.TabIndex = 4;
            quantityInput.Value = new decimal(new int[] { 1, 0, 0, 0 });
            //
            // purchasedCheckBox
            //
            purchasedCheckBox.AutoSize = true;
            purchasedCheckBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            purchasedCheckBox.Location = new Point(338, 428);
            purchasedCheckBox.Name = "purchasedCheckBox";
            purchasedCheckBox.Size = new Size(81, 19);
            purchasedCheckBox.TabIndex = 5;
            purchasedCheckBox.Text = "&Purchased";
            purchasedCheckBox.UseVisualStyleBackColor = true;
            //
            // addButton
            //
            addButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            addButton.Location = new Point(24, 472);
            addButton.Name = "addButton";
            addButton.Size = new Size(90, 32);
            addButton.TabIndex = 6;
            addButton.Text = "&Add";
            addButton.UseVisualStyleBackColor = true;
            addButton.Click += AddButton_Click;
            //
            // removeButton
            //
            removeButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            removeButton.Location = new Point(224, 472);
            removeButton.Name = "removeButton";
            removeButton.Size = new Size(90, 32);
            removeButton.TabIndex = 8;
            removeButton.Text = "&Remove";
            removeButton.UseVisualStyleBackColor = true;
            removeButton.Click += RemoveButton_Click;
            //
            // saveButton
            //
            saveButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            saveButton.Location = new Point(124, 472);
            saveButton.Name = "saveButton";
            saveButton.Size = new Size(90, 32);
            saveButton.TabIndex = 7;
            saveButton.Text = "&Save";
            saveButton.UseVisualStyleBackColor = true;
            saveButton.Click += SaveButton_Click;
            //
            // refreshButton
            //
            refreshButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            refreshButton.Location = new Point(676, 472);
            refreshButton.Name = "refreshButton";
            refreshButton.Size = new Size(100, 32);
            refreshButton.TabIndex = 9;
            refreshButton.Text = "Re&fresh";
            refreshButton.UseVisualStyleBackColor = true;
            refreshButton.Click += RefreshButton_Click;
            //
            // statusLabel
            //
            statusLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            statusLabel.Location = new Point(24, 521);
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(752, 55);
            statusLabel.TabIndex = 10;
            statusLabel.Text = "Open the list to load saved items.";
            //
            // itemNameTextBox
            //
            itemNameTextBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            itemNameTextBox.Location = new Point(24, 362);
            itemNameTextBox.AccessibleName = "Item name";
            itemNameTextBox.Name = "itemNameTextBox";
            itemNameTextBox.Size = new Size(752, 23);
            itemNameTextBox.TabIndex = 2;
            itemNameTextBox.TextChanged += ItemNameTextBox_TextChanged;
            //
            // ShoppingListForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 590);
            MinimumSize = new Size(816, 570);
            StartPosition = FormStartPosition.CenterParent;
            Controls.Add(titleLabel);
            Controls.Add(hintLabel);
            Controls.Add(itemNameTextBox);
            Controls.Add(statusLabel);
            Controls.Add(refreshButton);
            Controls.Add(saveButton);
            Controls.Add(removeButton);
            Controls.Add(addButton);
            Controls.Add(purchasedCheckBox);
            Controls.Add(quantityInput);
            Controls.Add(quantityLabel);
            Controls.Add(itemNameLabel);
            Controls.Add(itemsGrid);
            Name = "ShoppingListForm";
            Text = "Shopping list - Homeryn";
            ((System.ComponentModel.ISupportInitialize)itemsGrid).EndInit();
            ((System.ComponentModel.ISupportInitialize)quantityInput).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label titleLabel;
        private Label hintLabel;
        private DataGridView itemsGrid;
        private Label itemNameLabel;
        private Label quantityLabel;
        private NumericUpDown quantityInput;
        private CheckBox purchasedCheckBox;
        private Button addButton;
        private Button removeButton;
        private Button saveButton;
        private Button refreshButton;
        private Label statusLabel;
        private TextBox itemNameTextBox;
    }
}
