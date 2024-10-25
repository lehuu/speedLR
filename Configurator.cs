namespace SpeedLR
{
    public partial class Configurator : Form
    {
        private System.Windows.Forms.Timer connectionCheckTimer;
        private static int DEFAULT_PORT = 49000;

        public Configurator()
        {
            InitializeComponent();
            ConnectToServer();

            connectionCheckTimer = new System.Windows.Forms.Timer();
            connectionCheckTimer.Interval = 5000; // 5 seconds
            connectionCheckTimer.Tick += CheckConnection;
            connectionCheckTimer.Start();
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
    }


}
