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
            //
            // itemsGrid
            //
            itemsGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            itemsGrid.Location = new Point(17, 16);
            itemsGrid.Name = "itemsGrid";
            itemsGrid.Size = new Size(544, 220);
            itemsGrid.TabIndex = 0;
            itemsGrid.SelectionChanged += ItemsGrid_SelectionChanged;
            //
            // itemNameLabel
            //
            itemNameLabel.AutoSize = true;
            itemNameLabel.Location = new Point(18, 248);
            itemNameLabel.Name = "itemNameLabel";
            itemNameLabel.Size = new Size(64, 15);
            itemNameLabel.TabIndex = 1;
            itemNameLabel.Text = "&Item name";
            //
            // quantityLabel
            //
            quantityLabel.AutoSize = true;
            quantityLabel.Location = new Point(312, 248);
            quantityLabel.Name = "quantityLabel";
            quantityLabel.Size = new Size(53, 15);
            quantityLabel.TabIndex = 2;
            quantityLabel.Text = "&Quantity";
            //
            // quantityInput
            //
            quantityInput.Location = new Point(312, 273);
            quantityInput.Maximum = new decimal(new int[] { 999, 0, 0, 0 });
            quantityInput.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            quantityInput.Name = "quantityInput";
            quantityInput.Size = new Size(120, 23);
            quantityInput.TabIndex = 3;
            quantityInput.Value = new decimal(new int[] { 1, 0, 0, 0 });
            //
            // purchasedCheckBox
            //
            purchasedCheckBox.AutoSize = true;
            purchasedCheckBox.Location = new Point(451, 273);
            purchasedCheckBox.Name = "purchasedCheckBox";
            purchasedCheckBox.Size = new Size(81, 19);
            purchasedCheckBox.TabIndex = 4;
            purchasedCheckBox.Text = "&Purchased";
            purchasedCheckBox.UseVisualStyleBackColor = true;
            //
            // addButton
            //
            addButton.Location = new Point(18, 315);
            addButton.Name = "addButton";
            addButton.Size = new Size(75, 23);
            addButton.TabIndex = 5;
            addButton.Text = "&Add";
            addButton.UseVisualStyleBackColor = true;
            addButton.Click += AddButton_Click;
            //
            // removeButton
            //
            removeButton.Location = new Point(143, 315);
            removeButton.Name = "removeButton";
            removeButton.Size = new Size(75, 23);
            removeButton.TabIndex = 6;
            removeButton.Text = "&Remove";
            removeButton.UseVisualStyleBackColor = true;
            removeButton.Click += RemoveButton_Click;
            //
            // saveButton
            //
            saveButton.Location = new Point(268, 315);
            saveButton.Name = "saveButton";
            saveButton.Size = new Size(75, 23);
            saveButton.TabIndex = 7;
            saveButton.Text = "&Save";
            saveButton.UseVisualStyleBackColor = true;
            saveButton.Click += SaveButton_Click;
            //
            // refreshButton
            //
            refreshButton.Location = new Point(451, 315);
            refreshButton.Name = "refreshButton";
            refreshButton.Size = new Size(75, 23);
            refreshButton.TabIndex = 8;
            refreshButton.Text = "Re&fresh";
            refreshButton.UseVisualStyleBackColor = true;
            refreshButton.Click += RefreshButton_Click;
            //
            // statusLabel
            //
            statusLabel.Location = new Point(18, 361);
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(544, 64);
            statusLabel.TabIndex = 9;
            statusLabel.Text = "Open the list to load saved items.";
            //
            // itemNameTextBox
            //
            itemNameTextBox.Location = new Point(18, 273);
            itemNameTextBox.Name = "itemNameTextBox";
            itemNameTextBox.Size = new Size(269, 23);
            itemNameTextBox.TabIndex = 2;
            itemNameTextBox.TextChanged += ItemNameTextBox_TextChanged;
            //
            // ShoppingListForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
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
