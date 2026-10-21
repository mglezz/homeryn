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
            SuspendLayout();
            //
            // connectButton
            //
            connectButton.Location = new Point(24, 24);
            connectButton.Name = "connectButton";
            connectButton.Size = new Size(160, 36);
            connectButton.TabIndex = 0;
            connectButton.Text = "Check connection";
            connectButton.UseVisualStyleBackColor = true;
            connectButton.Click += ConnectButton_Click;
            //
            // MainForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(connectButton);
            Name = "MainForm";
            Text = "Homeryn";
            ResumeLayout(false);
        }

        #endregion

        private Button connectButton;
    }
}
