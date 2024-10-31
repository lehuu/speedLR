﻿using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SpeedLR
{
    public class Connector
    {
        private static Connector _instance;
        private Socket _clientSocket;
        private DateTime _lastSendTime = DateTime.MinValue;
        private double _maxRequestsPerSecond = 16;
        private Connector()
        { }

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

        public async Task<bool> IsConnected()
        {
            if (_clientSocket == null || !_clientSocket.Connected)
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

                _clientSocket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                await _clientSocket.ConnectAsync(endpoint);
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
            var currentTime = DateTime.Now;
            var elapsed = currentTime - _lastSendTime;
            if (elapsed < TimeSpan.FromMilliseconds(1000 / _maxRequestsPerSecond)) // 1000 ms / 16 = 62.5 ms
            {
                return;
            }
            _lastSendTime = DateTime.Now;

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
            }
        }
    }
}
