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
            connectButton = new Button();
            shoppingListButton = new Button();
            choresButton = new Button();
            startServerButton = new Button();
            SuspendLayout();
            //
            // connectButton
            //
            connectButton.Location = new Point(404, 224);
            connectButton.Name = "connectButton";
            connectButton.Size = new Size(118, 34);
            connectButton.TabIndex = 0;
            connectButton.Text = "Check connection";
            connectButton.UseVisualStyleBackColor = true;
            connectButton.Click += ConnectButton_Click;
            //
            // shoppingListButton
            //
            shoppingListButton.Location = new Point(49, 98);
            shoppingListButton.Name = "shoppingListButton";
            shoppingListButton.Size = new Size(160, 62);
            shoppingListButton.TabIndex = 1;
            shoppingListButton.Text = "Shopping list";
            shoppingListButton.UseVisualStyleBackColor = true;
            shoppingListButton.Click += ShoppingListButton_Click;
            //
            // choresButton
            //
            choresButton.Location = new Point(321, 98);
            choresButton.Name = "choresButton";
            choresButton.Size = new Size(160, 62);
            choresButton.TabIndex = 2;
            choresButton.Text = "Chores";
            choresButton.UseVisualStyleBackColor = true;
            choresButton.Click += ChoresButton_Click;
            //
            // startServerButton
            //
            startServerButton.Location = new Point(12, 12);
            startServerButton.Name = "startServerButton";
            startServerButton.Size = new Size(118, 25);
            startServerButton.TabIndex = 3;
            startServerButton.Text = "Start server";
            startServerButton.UseVisualStyleBackColor = true;
            startServerButton.Click += StartServerButton_Click;
            //
            // MainForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(534, 270);
            Controls.Add(startServerButton);
            Controls.Add(choresButton);
            Controls.Add(shoppingListButton);
            Controls.Add(connectButton);
            Name = "MainForm";
            Text = "Homeryn";
            ResumeLayout(false);
        }

        #endregion

        private Button connectButton;
        private Button shoppingListButton;
        private Button choresButton;
        private Button startServerButton;
    }
}
