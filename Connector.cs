using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SpeedLR
{
    public class Connector
    {
        private static Connector instance;
        private Socket clientSocket;

        private Connector()
        { }

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

        public async Task<bool> IsConnected()
        {
            if (clientSocket == null || !clientSocket.Connected)
            {
                return false;
            }

            try
            {
                await SendCommandAsync("Test");
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task Connect(string address, int port)
        {
            try
            {
                CloseConnection();
                await Task.Delay(1000);

                IPHostEntry host = Dns.GetHostEntry(address);
                IPAddress ipAddress = host.AddressList[0];
                var endpoint = new IPEndPoint(ipAddress, port);

                clientSocket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                await clientSocket.ConnectAsync(endpoint);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error connecting to server: {ex.Message}");
            }
        }

        public async Task Connect(int port)
        {
            try
            {
                await Connect("127.0.0.1", port);
            }
            catch (Exception)
            {
                try
                {
                    await Connect("localhost", port);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error connecting to server: {ex.Message}");
                }
            }
        }

        public async Task SendCommandAsync(string command)
        {
            if (clientSocket == null || !clientSocket.Connected)
            {
                throw new Exception("Not connected to the server.");
            }

            try
            {
                command += "\n";
                byte[] data = Encoding.ASCII.GetBytes(command);
                await clientSocket.SendAsync(data, SocketFlags.None);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error sending command: {ex.Message}");
            }
        }

        public void CloseConnection()
        {
            if (clientSocket != null)
            {
                if (clientSocket.Connected)
                {
                    clientSocket.Shutdown(SocketShutdown.Both);
                }
                clientSocket.Close();
                clientSocket = null;
            }
        }
    }
}
