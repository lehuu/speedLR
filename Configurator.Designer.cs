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
            portButton = new Button();
            connectButton = new Button();
            SuspendLayout();
            // 
            // portButton
            // 
            portButton.Location = new Point(139, 404);
            portButton.Name = "portButton";
            portButton.Size = new Size(192, 34);
            portButton.TabIndex = 0;
            portButton.Text = "Change Port";
            portButton.UseVisualStyleBackColor = true;
            portButton.Click += button1_Click_1;
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
            connectButton.Click += ConnectButton_Click;
            // 
            // Configurator
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(connectButton);
            Controls.Add(portButton);
            Name = "Configurator";
            Text = "Form1";
            ResumeLayout(false);
        }

        #endregion

        private Button portButton;
        private Button connectButton;
    }
}
