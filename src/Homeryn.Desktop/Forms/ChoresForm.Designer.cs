namespace Homeryn.Desktop.Forms
{
    partial class ChoresForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && !IsDisposed)
            {
                DisposeRequests();
            }
            if (disposing && components != null)
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            titleLabel = new Label();
            hintLabel = new Label();
            itemsGrid = new DataGridView();
            itemNameLabel = new Label();
            itemNameTextBox = new TextBox();
            assigneeLabel = new Label();
            assigneeTextBox = new TextBox();
            dueDateLabel = new Label();
            dueDatePicker = new DateTimePicker();
            completedCheckBox = new CheckBox();
            newButton = new Button();
            addButton = new Button();
            saveButton = new Button();
            removeButton = new Button();
            refreshButton = new Button();
            statusLabel = new Label();
            ((System.ComponentModel.ISupportInitialize)itemsGrid).BeginInit();
            SuspendLayout();
            // titleLabel
            titleLabel.AutoSize = true;
            titleLabel.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            titleLabel.Location = new Point(24, 18);
            titleLabel.Name = "titleLabel";
            titleLabel.Text = "Chores";
            // hintLabel
            hintLabel.AutoSize = true;
            hintLabel.Location = new Point(25, 56);
            hintLabel.Name = "hintLabel";
            hintLabel.Text = "Select a chore to edit it. Use New to clear the editor and start another chore.";
            // itemsGrid
            itemsGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            itemsGrid.BackgroundColor = SystemColors.Window;
            itemsGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            itemsGrid.Location = new Point(24, 85);
            itemsGrid.Name = "itemsGrid";
            itemsGrid.Size = new Size(752, 235);
            itemsGrid.TabIndex = 0;
            itemsGrid.AccessibleName = "Chores list";
            itemsGrid.SelectionChanged += ItemsGrid_SelectionChanged;
            // itemNameLabel
            itemNameLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            itemNameLabel.AutoSize = true;
            itemNameLabel.Location = new Point(24, 338);
            itemNameLabel.Name = "itemNameLabel";
            itemNameLabel.Text = "&Chore name";
            itemNameLabel.TabIndex = 1;
            // itemNameTextBox
            itemNameTextBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            itemNameTextBox.Location = new Point(24, 362);
            itemNameTextBox.MaxLength = 80;
            itemNameTextBox.Name = "itemNameTextBox";
            itemNameTextBox.Size = new Size(752, 23);
            itemNameTextBox.TabIndex = 2;
            itemNameTextBox.AccessibleName = "Chore name";
            itemNameTextBox.TextChanged += ItemNameTextBox_TextChanged;
            // assigneeLabel
            assigneeLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            assigneeLabel.AutoSize = true;
            assigneeLabel.Location = new Point(24, 402);
            assigneeLabel.Name = "assigneeLabel";
            assigneeLabel.Text = "Responsible &person (optional)";
            assigneeLabel.TabIndex = 3;
            // assigneeTextBox
            assigneeTextBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            assigneeTextBox.Location = new Point(24, 426);
            assigneeTextBox.MaxLength = 60;
            assigneeTextBox.Name = "assigneeTextBox";
            assigneeTextBox.Size = new Size(290, 23);
            assigneeTextBox.TabIndex = 4;
            assigneeTextBox.AccessibleName = "Responsible person (optional)";
            // dueDateLabel
            dueDateLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            dueDateLabel.AutoSize = true;
            dueDateLabel.Location = new Point(338, 402);
            dueDateLabel.Name = "dueDateLabel";
            dueDateLabel.Text = "&Due date (check to set)";
            dueDateLabel.TabIndex = 5;
            // dueDatePicker
            dueDatePicker.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            dueDatePicker.Format = DateTimePickerFormat.Short;
            dueDatePicker.ShowCheckBox = true;
            dueDatePicker.Checked = false;
            dueDatePicker.MinDate = new DateTime(1753, 1, 1);
            dueDatePicker.MaxDate = new DateTime(9998, 12, 31);
            dueDatePicker.Location = new Point(338, 426);
            dueDatePicker.Name = "dueDatePicker";
            dueDatePicker.Size = new Size(200, 23);
            dueDatePicker.TabIndex = 6;
            dueDatePicker.AccessibleName = "Due date (optional)";
            // completedCheckBox
            completedCheckBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            completedCheckBox.AutoSize = true;
            completedCheckBox.Location = new Point(565, 428);
            completedCheckBox.Name = "completedCheckBox";
            completedCheckBox.Text = "Co&mpleted";
            completedCheckBox.TabIndex = 7;
            completedCheckBox.UseVisualStyleBackColor = true;
            // newButton
            newButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            newButton.Location = new Point(24, 472);
            newButton.Name = "newButton";
            newButton.Size = new Size(90, 32);
            newButton.Text = "&New";
            newButton.TabIndex = 8;
            newButton.UseVisualStyleBackColor = true;
            newButton.Click += NewButton_Click;
            // addButton
            addButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            addButton.Location = new Point(124, 472);
            addButton.Name = "addButton";
            addButton.Size = new Size(90, 32);
            addButton.Text = "&Add";
            addButton.TabIndex = 9;
            addButton.UseVisualStyleBackColor = true;
            addButton.Click += AddButton_Click;
            // saveButton
            saveButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            saveButton.Location = new Point(224, 472);
            saveButton.Name = "saveButton";
            saveButton.Size = new Size(90, 32);
            saveButton.Text = "&Save";
            saveButton.TabIndex = 10;
            saveButton.UseVisualStyleBackColor = true;
            saveButton.Click += SaveButton_Click;
            // removeButton
            removeButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            removeButton.Location = new Point(324, 472);
            removeButton.Name = "removeButton";
            removeButton.Size = new Size(90, 32);
            removeButton.Text = "&Remove";
            removeButton.TabIndex = 11;
            removeButton.UseVisualStyleBackColor = true;
            removeButton.Click += RemoveButton_Click;
            // refreshButton
            refreshButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            refreshButton.Location = new Point(676, 472);
            refreshButton.Name = "refreshButton";
            refreshButton.Size = new Size(100, 32);
            refreshButton.Text = "Re&fresh";
            refreshButton.TabIndex = 12;
            refreshButton.UseVisualStyleBackColor = true;
            refreshButton.Click += RefreshButton_Click;
            // statusLabel
            statusLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            statusLabel.Location = new Point(24, 521);
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(752, 55);
            statusLabel.TabIndex = 13;
            statusLabel.Text = "Loading chores...";
            // ChoresForm
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 590);
            MinimumSize = new Size(816, 570);
            StartPosition = FormStartPosition.CenterParent;
            Controls.Add(titleLabel);
            Controls.Add(hintLabel);
            Controls.Add(itemsGrid);
            Controls.Add(itemNameLabel);
            Controls.Add(itemNameTextBox);
            Controls.Add(assigneeLabel);
            Controls.Add(assigneeTextBox);
            Controls.Add(dueDateLabel);
            Controls.Add(dueDatePicker);
            Controls.Add(completedCheckBox);
            Controls.Add(newButton);
            Controls.Add(addButton);
            Controls.Add(saveButton);
            Controls.Add(removeButton);
            Controls.Add(refreshButton);
            Controls.Add(statusLabel);
            Name = "ChoresForm";
            Text = "Chores - Homeryn";
            ((System.ComponentModel.ISupportInitialize)itemsGrid).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label titleLabel;
        private Label hintLabel;
        private DataGridView itemsGrid;
        private Label itemNameLabel;
        private TextBox itemNameTextBox;
        private Label assigneeLabel;
        private TextBox assigneeTextBox;
        private Label dueDateLabel;
        private DateTimePicker dueDatePicker;
        private CheckBox completedCheckBox;
        private Button newButton;
        private Button addButton;
        private Button saveButton;
        private Button removeButton;
        private Button refreshButton;
        private Label statusLabel;
    }
}
