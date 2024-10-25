namespace SpeedLR
{
    partial class Configurator
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
            portButton = new Button();
            SuspendLayout();
            // 
            // connectButton
            // 
            connectButton.BackColor = Color.IndianRed;
            connectButton.Location = new Point(21, 404);
            connectButton.Name = "connectButton";
            connectButton.Size = new Size(112, 34);
            connectButton.TabIndex = 1;
            connectButton.Text = "Reconnect";
            connectButton.UseVisualStyleBackColor = false;
            connectButton.Click += connectButton_Click;
            // 
            // portButton
            // 
            portButton.Location = new Point(139, 404);
            portButton.Name = "portButton";
            portButton.Size = new Size(112, 34);
            portButton.TabIndex = 2;
            portButton.Text = "Port";
            portButton.UseVisualStyleBackColor = true;
            portButton.Click += portButton_Click;
            // 
            // Configurator
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(portButton);
            Controls.Add(connectButton);
            Name = "Configurator";
            Text = "SpeedLR";
            ResumeLayout(false);
        }

        #endregion
        private Button connectButton;
        private Button portButton;
    }
}
