namespace SpeedLR
{
    public partial class Configurator : Form
    {
        private System.Windows.Forms.Timer connectionCheckTimer;

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
            try
            {
                this.connectButton.Text = "Connecting...";
                this.connectButton.BackColor = Color.Blue;

                await Connector.Instance.Connect();

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


        private void button1_Click_1(object sender, EventArgs e)
        {

        }


        private void ConnectButton_Click(object sender, EventArgs e)
        {
            if (!Connector.Instance.IsConnected)
            {
                ConnectToServer();
            }
        }
    }


}
