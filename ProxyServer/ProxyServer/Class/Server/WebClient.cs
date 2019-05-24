using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ProxyServer.Class
{
    public class SWebClient
    {
        public static int CAPACITY { get; } = 5242880;
        public Dictionary<string, string> Headers { get; set; }
        private Socket Socket { get; set; }
        private HttpMessage Message { get; set; }
        private byte[] Bytes { get; set; }
        private Uri Address { get; set; }
        private NetworkStream Stream;

        public SWebClient()
        {
            Headers = new Dictionary<string, string>();
            Socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            Bytes = new byte[CAPACITY];
        }

        public SWebClient(string address)
        {
            Headers = new Dictionary<string, string>();
            Socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            Bytes = new byte[CAPACITY];
            Address = new Uri(address);
            Socket.Connect(Address.Host, Address.Port);
            Stream = new NetworkStream(Socket);
            //Socket.Connect(uri.)
        }

        public void Connect(string host, int port)
        {
            Socket.Connect(host, port);
            if (Socket.Connected)
            {
                Stream = new NetworkStream(Socket);
            }
        }

        public void Send(string message)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(message);
            Stream.Write(bytes, 0, bytes.Length);
        }

        private void Receive()
        {
            int receiv = Stream.Read(Bytes, 0, CAPACITY);

            if (receiv > 0)
            {

            }
        }
    }
}
