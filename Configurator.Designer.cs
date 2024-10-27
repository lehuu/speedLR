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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Configurator));
            connectButton = new Button();
            portButton = new Button();
            SuspendLayout();
            // 
            // connectButton
            // 
            connectButton.BackColor = Color.IndianRed;
            connectButton.Location = new Point(17, 323);
            connectButton.Margin = new Padding(2, 2, 2, 2);
            connectButton.Name = "connectButton";
            connectButton.Size = new Size(90, 27);
            connectButton.TabIndex = 1;
            connectButton.Text = "Reconnect";
            connectButton.UseVisualStyleBackColor = false;
            connectButton.Click += connectButton_Click;
            // 
            // portButton
            // 
            portButton.Location = new Point(111, 323);
            portButton.Margin = new Padding(2, 2, 2, 2);
            portButton.Name = "portButton";
            portButton.Size = new Size(90, 27);
            portButton.TabIndex = 2;
            portButton.Text = "Port";
            portButton.UseVisualStyleBackColor = true;
            portButton.Click += portButton_Click;
            // 
            // Configurator
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(640, 360);
            Controls.Add(portButton);
            Controls.Add(connectButton);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(2, 2, 2, 2);
            MaximizeBox = false;
            Name = "Configurator";
            Text = "SpeedLR";
            FormClosing += Configurator_FormClosing;
            ResumeLayout(false);
        }

        #endregion
        private Button connectButton;
        private Button portButton;
    }
}
