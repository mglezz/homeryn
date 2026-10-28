namespace Homeryn.Desktop
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            titleLabel = new Label();
            hintLabel = new Label();
            navigationLayout = new TableLayoutPanel();
            shoppingPanel = new Panel();
            shoppingTitleLabel = new Label();
            shoppingDescriptionLabel = new Label();
            shoppingListButton = new Button();
            choresPanel = new Panel();
            choresTitleLabel = new Label();
            choresDescriptionLabel = new Label();
            choresButton = new Button();
            startServerButton = new Button();
            connectButton = new Button();
            statusLabel = new Label();
            navigationLayout.SuspendLayout();
            shoppingPanel.SuspendLayout();
            choresPanel.SuspendLayout();
            SuspendLayout();
            // titleLabel
            titleLabel.AutoSize = true;
            titleLabel.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            titleLabel.Location = new Point(24, 18);
            titleLabel.Name = "titleLabel";
            titleLabel.Text = "Homeryn";
            // hintLabel
            hintLabel.AutoSize = true;
            hintLabel.Location = new Point(25, 56);
            hintLabel.Name = "hintLabel";
            hintLabel.Text = "Your home, in sync. Keep shopping and shared responsibilities in one place.";
            // navigationLayout
            navigationLayout.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            navigationLayout.ColumnCount = 2;
            navigationLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            navigationLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            navigationLayout.Controls.Add(shoppingPanel, 0, 0);
            navigationLayout.Controls.Add(choresPanel, 1, 0);
            navigationLayout.Location = new Point(24, 85);
            navigationLayout.Margin = new Padding(0);
            navigationLayout.Name = "navigationLayout";
            navigationLayout.RowCount = 1;
            navigationLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            navigationLayout.Size = new Size(752, 215);
            navigationLayout.TabIndex = 0;
            // shoppingPanel
            shoppingPanel.BackColor = SystemColors.Window;
            shoppingPanel.BorderStyle = BorderStyle.FixedSingle;
            shoppingPanel.Controls.Add(shoppingTitleLabel);
            shoppingPanel.Controls.Add(shoppingDescriptionLabel);
            shoppingPanel.Controls.Add(shoppingListButton);
            shoppingPanel.Dock = DockStyle.Fill;
            shoppingPanel.Margin = new Padding(0, 0, 12, 0);
            shoppingPanel.Name = "shoppingPanel";
            shoppingPanel.Size = new Size(364, 215);
            shoppingPanel.TabIndex = 0;
            // shoppingTitleLabel
            shoppingTitleLabel.AutoSize = true;
            shoppingTitleLabel.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            shoppingTitleLabel.Location = new Point(20, 20);
            shoppingTitleLabel.Name = "shoppingTitleLabel";
            shoppingTitleLabel.Text = "Shopping list";
            // shoppingDescriptionLabel
            shoppingDescriptionLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            shoppingDescriptionLabel.Location = new Point(20, 58);
            shoppingDescriptionLabel.Name = "shoppingDescriptionLabel";
            shoppingDescriptionLabel.Size = new Size(320, 55);
            shoppingDescriptionLabel.Text = "Plan your next shop, update quantities and keep track of what has been purchased.";
            // shoppingListButton
            shoppingListButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            shoppingListButton.Location = new Point(20, 160);
            shoppingListButton.Name = "shoppingListButton";
            shoppingListButton.Size = new Size(140, 32);
            shoppingListButton.TabIndex = 0;
            shoppingListButton.Text = "&Shopping list";
            shoppingListButton.UseVisualStyleBackColor = true;
            shoppingListButton.Click += ShoppingListButton_Click;
            // choresPanel
            choresPanel.BackColor = SystemColors.Window;
            choresPanel.BorderStyle = BorderStyle.FixedSingle;
            choresPanel.Controls.Add(choresTitleLabel);
            choresPanel.Controls.Add(choresDescriptionLabel);
            choresPanel.Controls.Add(choresButton);
            choresPanel.Dock = DockStyle.Fill;
            choresPanel.Margin = new Padding(12, 0, 0, 0);
            choresPanel.Name = "choresPanel";
            choresPanel.Size = new Size(364, 215);
            choresPanel.TabIndex = 1;
            // choresTitleLabel
            choresTitleLabel.AutoSize = true;
            choresTitleLabel.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            choresTitleLabel.Location = new Point(20, 20);
            choresTitleLabel.Name = "choresTitleLabel";
            choresTitleLabel.Text = "Chores";
            // choresDescriptionLabel
            choresDescriptionLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            choresDescriptionLabel.Location = new Point(20, 58);
            choresDescriptionLabel.Name = "choresDescriptionLabel";
            choresDescriptionLabel.Size = new Size(320, 55);
            choresDescriptionLabel.Text = "Share the housework, set due dates and see what still needs to be done.";
            // choresButton
            choresButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            choresButton.Location = new Point(20, 160);
            choresButton.Name = "choresButton";
            choresButton.Size = new Size(140, 32);
            choresButton.TabIndex = 0;
            choresButton.Text = "&Chores";
            choresButton.UseVisualStyleBackColor = true;
            choresButton.Click += ChoresButton_Click;
            // startServerButton
            startServerButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            startServerButton.Location = new Point(24, 332);
            startServerButton.Name = "startServerButton";
            startServerButton.Size = new Size(120, 32);
            startServerButton.TabIndex = 1;
            startServerButton.Text = "S&tart server";
            startServerButton.UseVisualStyleBackColor = true;
            startServerButton.Click += StartServerButton_Click;
            // connectButton
            connectButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            connectButton.Location = new Point(636, 332);
            connectButton.Name = "connectButton";
            connectButton.Size = new Size(140, 32);
            connectButton.TabIndex = 2;
            connectButton.Text = "Check co&nnection";
            connectButton.UseVisualStyleBackColor = true;
            connectButton.Click += ConnectButton_Click;
            // statusLabel
            statusLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            statusLabel.Location = new Point(24, 381);
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(752, 55);
            statusLabel.Text = "Start the server, then open a list. Use Refresh inside each list to load other changes.";
            // MainForm
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            MinimumSize = new Size(816, 489);
            StartPosition = FormStartPosition.CenterScreen;
            Controls.Add(titleLabel);
            Controls.Add(hintLabel);
            Controls.Add(navigationLayout);
            Controls.Add(startServerButton);
            Controls.Add(connectButton);
            Controls.Add(statusLabel);
            Name = "MainForm";
            Text = "Homeryn";
            navigationLayout.ResumeLayout(false);
            shoppingPanel.ResumeLayout(false);
            shoppingPanel.PerformLayout();
            choresPanel.ResumeLayout(false);
            choresPanel.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label titleLabel;
        private Label hintLabel;
        private TableLayoutPanel navigationLayout;
        private Panel shoppingPanel;
        private Label shoppingTitleLabel;
        private Label shoppingDescriptionLabel;
        private Button shoppingListButton;
        private Panel choresPanel;
        private Label choresTitleLabel;
        private Label choresDescriptionLabel;
        private Button choresButton;
        private Button startServerButton;
        private Button connectButton;
        private Label statusLabel;
    }
}
