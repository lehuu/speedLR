using System.Net;
using System.Net.Sockets;
using System.Text;
using Timer = System.Timers.Timer;

namespace SpeedLR
{
    public class Connector
    {
        public enum ConnectionStatus
        {
            DISCONNECTED,
            CONNECTING,
            CONNECTED
        }

        private static Connector _instance;
        private Socket _clientSocket;
        private DateTime _lastSendTime = DateTime.MinValue;
        private double _maxRequestsPerSecond = 16;
        private Timer _connectionTimer;

        public event EventHandler<ConnectionStatus> ConnectionChanged;

        private Connector()
        {
            InitializeTimer();
        }

        public static Connector Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Connector();
                }
                return _instance;
            }
        }

        private ConnectionStatus _status;

        public ConnectionStatus Status
        {
            get { return _status; }
            set
            {
                if (_status != value)
                {
                    _status = value;
                    ConnectionChanged?.Invoke(this, value);
                }
            }
        }

        private void InitializeTimer()
        {
            _connectionTimer = new Timer(10000); // 10 seconds in milliseconds
            _connectionTimer.Elapsed += (sender, e) => CheckConnection();
            _connectionTimer.AutoReset = true;
            _connectionTimer.Start();
        }

        private async void CheckConnection()
        {
            if (_clientSocket == null || !_clientSocket.Connected)
            {
                Status = ConnectionStatus.DISCONNECTED;
            }

            try
            {
                await SendCommandAsync("Test");
                Status = ConnectionStatus.CONNECTED;
            }
            catch (Exception)
            {
                Status = ConnectionStatus.DISCONNECTED;
            }
        }

        private async Task Connect(string address, int port)
        {
            try
            {
                CloseConnection();
                Status = ConnectionStatus.CONNECTING;
                await Task.Delay(1000);
				if (IPAddress.TryParse(address, out IPAddress ipAddress))
				{
					var endpoint = new IPEndPoint(ipAddress, port);
					_clientSocket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
					await _clientSocket.ConnectAsync(endpoint);
				}
				else
				{
					throw new Exception($"Invalid IP address: {address}");
				}
			}
            catch (Exception ex)
            {
                throw new Exception($"Error connecting to server: {ex.Message}");
            }
        }

        public async Task Connect(int port)
        {
            Status = ConnectionStatus.CONNECTING;
            try
            {
                await Connect("127.0.0.1", port);
                Status = ConnectionStatus.CONNECTED;
            }
            catch (Exception)
            {
                try
                {
                    await Connect("localhost", port);
                    Status = ConnectionStatus.CONNECTED;
                }
                catch (Exception ex)
                {
                    Status = ConnectionStatus.DISCONNECTED;
                }
            }
        }

        public async Task SendCommandAsync(string command)
        {
            var currentTime = DateTime.Now;
            var elapsed = currentTime - _lastSendTime;
            if (elapsed < TimeSpan.FromMilliseconds(1000 / _maxRequestsPerSecond)) // 1000 ms / 16 = 62.5 ms
            {
                return;
            }
            _lastSendTime = DateTime.Now;

            ResetTimer(); // Reset the timer on command send

            if (_clientSocket == null || !_clientSocket.Connected)
            {
                throw new Exception("Not connected to the server.");
            }

            try
            {
                command += "\n";
                byte[] data = Encoding.ASCII.GetBytes(command);
                await _clientSocket.SendAsync(data, SocketFlags.None);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error sending command: {ex.Message}");
            }
        }

        private void ResetTimer()
        {
            _connectionTimer.Stop();
            _connectionTimer.Start();
        }

        public void CloseConnection()
        {
            if (_clientSocket != null)
            {
                if (_clientSocket.Connected)
                {
                    _clientSocket.Shutdown(SocketShutdown.Both);
                }
                _clientSocket.Close();
                _clientSocket = null;
                Status = ConnectionStatus.DISCONNECTED;
            }
        }
    }
}
