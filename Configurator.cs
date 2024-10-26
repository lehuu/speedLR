namespace SpeedLR
{
    public partial class Configurator : Form
    {
        private System.Windows.Forms.Timer connectionCheckTimer;
        private static int DEFAULT_PORT = 49000;
        private NotifyIcon notifyIcon;

        public Configurator()
        {
            InitializeComponent();
            ConnectToServer();

            // Check connection periodically
            connectionCheckTimer = new System.Windows.Forms.Timer();
            connectionCheckTimer.Interval = 5000; // 5 seconds
            connectionCheckTimer.Tick += CheckConnection;
            connectionCheckTimer.Start();

            // init context menu
            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = new Icon("wheel.ico"); // Replace with your icon file
            notifyIcon.Text = this.Text;
            notifyIcon.Visible = true;

            // Create a context menu for the NotifyIcon
            ContextMenuStrip contextMenu = new ContextMenuStrip();
            ToolStripMenuItem exitMenuItem = new ToolStripMenuItem("Exit");
            exitMenuItem.Click += exitMenuItem_Click;
            contextMenu.Items.Add(exitMenuItem);

            notifyIcon.ContextMenuStrip = contextMenu;
        }

        private void CheckConnection(object sender, EventArgs e)
        {
            if (Connector.Instance.IsConnected)
            {
                this.connectButton.BackColor = Color.Green;
                this.connectButton.Text = "Connected";
            }
            else
            {
                this.connectButton.BackColor = Color.Red;
                this.connectButton.Text = "Reconnect";
            }
        }

        private async void ConnectToServer()
        {
            this.portButton.Text = "Port: " + DEFAULT_PORT;

            try
            {
                this.connectButton.Text = "Connecting...";
                this.connectButton.BackColor = Color.Blue;

                await Connector.Instance.Connect(DEFAULT_PORT);

                this.connectButton.BackColor = Color.Green;
                this.connectButton.Text = "Connected";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                this.connectButton.BackColor = Color.Red;
                this.connectButton.Text = "Reconnect";
            }
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            if (!Connector.Instance.IsConnected)
            {
                ConnectToServer();
            }
        }

        private void portButton_Click(object sender, EventArgs e)
        {

        }

        private void exitMenuItem_Click(object sender, EventArgs e)
        {
            // Close the application
            notifyIcon.Dispose(); // Release the notify icon resource
            Application.Exit();
        }

        private void Configurator_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                // Minimize to tray instead of closing
                e.Cancel = true;
                Hide();
            }
            else
            {
                // Allow closing when the application is actually exiting
                Connector.Instance.CloseConnection();
            }
        }
    }


}
