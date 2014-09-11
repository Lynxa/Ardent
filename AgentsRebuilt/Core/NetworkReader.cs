using System;
using System.Globalization;
using System.Net.Sockets;
using System.Text;

namespace AgentsRebuilt
{
    public class NetworkReader
    {
        private const char TERMINATOR = '\n';
        private readonly TcpClient _client;
        private byte[] _buffer = new byte[10240];
        private String _data;

        public delegate void OnDataHandler(string message);
        public event OnDataHandler OnDataRevieved;


        public NetworkReader()
        {
            _client = new TcpClient();
        }

        public void Connect(string hostname, int port)
        {
            _client.Connect(hostname, port);
            NetworkStream stream = new NetworkStream(_client.Client);
            _data = "";
            ReadNetworkData(stream);
        }

        private void ReadNetworkData(NetworkStream stream)
        {
            stream.BeginRead(_buffer, 0, _buffer.Length, ar =>
                {
                    ParseBuffer(stream.EndRead(ar));
                    ReadNetworkData(stream);
                }, stream);
        }

        private void ParseBuffer(int numberOfBytesRead)
        {
            _data += Encoding.ASCII.GetString(_buffer, 0, numberOfBytesRead);
            string[] strings = _data.Split(TERMINATOR);
            if (strings.Length > 1)
            {
                for (int i = 0; i < strings.Length - 1; i++)
                {
                    OnDataRevieved(strings[i]);
                }
            }
            if (_data.EndsWith(TERMINATOR.ToString(CultureInfo.InvariantCulture)))
            {
                OnDataRevieved(strings[strings.Length-1]);
                _data = "";
            }
            else
            {
                _data = strings[strings.Length - 1];
            }
        }
    }
}
