using System.Net.Sockets;
using System.Text;

namespace SpeedLR
{
    public partial class Configurator : Form
    {
        private TcpClient client;
        private NetworkStream stream;

        private static string IP_ADDRESS = "localhost";
        private static int PORT = 49000;

        public Configurator()
        {
            InitializeComponent();
            ConnectToServer();
        }

        private void ConnectToServer()
        {
            try
            {
                client = new TcpClient(IP_ADDRESS, PORT);
                stream = client.GetStream();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error connecting to server: " + ex.Message);
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            try
            {
                string command = "Exposure=+2%";
                byte[] data = Encoding.ASCII.GetBytes(command + "\n");
                stream.Write(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        private void Configurator_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (stream != null)
                stream.Close();
            if (client != null)
                client.Close();
        }
    }


}
