using System.Net.Sockets;
using System.Text;

namespace SpeedLR
{
    public class Connector
    {
        private static string IP_ADDRESS = "localhost";
        private static Connector instance;
        private TcpClient client;
        private NetworkStream stream;

        private Connector() {}

        public static Connector Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Connector();
                }
                return instance;
            }
        }
        public bool IsConnected
        {
            get
            {
                if (stream == null || client == null || !client.Connected)
                {
                    return false;
                }

                try
                {
                    SendCommand("Test");
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public async Task Connect(int port)
        {
            try
            {
                CloseConnection();
                await Task.Delay(1000);

                client = new TcpClient(IP_ADDRESS, port);
                stream = client.GetStream();
            }
            catch (Exception ex)
            {
                throw new Exception("Error connecting to server: " + ex.Message);
            }
        }

        public void SendCommand(string command)
        {
            try
            {
                if (client != null && client.Connected)
                {
                    command += "\n";

                    byte[] data = Encoding.ASCII.GetBytes(command);
                    stream.Write(data, 0, data.Length);
                }
                else
                {
                    throw new Exception("Not connected to server.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error sending command: " + ex.Message);
            }
        }

        public void CloseConnection()
        {
            if (stream != null)
                stream.Close();
            if (client != null)
                client.Close();
        }
    }
}
