using ProxyServer.AsyncSocket.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProxyServer.Class
{
    public class SWebClient
    {
        public BridgeConnection Parent { get; set; }
        public static int CAPACITY { get; } = 5242880;
        private ManualResetEvent SendDone = new ManualResetEvent(false);
        private ManualResetEvent ReceiveDone = new ManualResetEvent(false);
        public Dictionary<string, string> Headers { get; set; }
        private Socket Socket { get; set; }
        private HttpMessage Message { get; set; }
        private List<byte> Bytes { get; set; }
        private Uri Address { get; set; }
        private NetworkStream Stream = null;

        public SWebClient(BridgeConnection parent)
        {
            Headers = new Dictionary<string, string>();
            Socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            Bytes = new List<byte>();

            Socket.ReceiveBufferSize = 262140;
            Socket.SendBufferSize = 262140;
            Parent = parent;
        }

        public SWebClient(BridgeConnection parent, string address)
        {
            Headers = new Dictionary<string, string>();
            Socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            Bytes = new List<byte>();
            Address = new Uri(address);

            Socket.Connect(Address.Host, Address.Port);
            Socket.ReceiveBufferSize = 262140;
            Socket.SendBufferSize = 262140;
            Parent = parent;
        }

        public void Connect(string host, int port)
        {
            if (!Socket.Connected)
            {
                Socket.Connect(host, port);
                if (Socket.Connected)
                {
                    Stream = new NetworkStream(Socket);
                }
            }
        }

        public void Connect(Uri uri)
        {
            Socket.Connect(uri.Host, 80);
            Address = uri;
        }

        public void Read()
        {
            StateObject state = new StateObject();
            state.workSocket = Socket;
            SocketError error;
            state.workSocket.BeginReceive(state.buffer, 0, state.buffer.Length, SocketFlags.None, out error, new AsyncCallback(ReadCallback), state);
        }

        public void ReadCallback(IAsyncResult ar)
        {
            string content = string.Empty;
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            if (!handler.Connected)
                return;

            int bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                Console.WriteLine("Read from webserver: " + bytesRead);
                state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                //Bytes.AddRange(state.buffer);
            }
            
            if (bytesRead < handler.ReceiveBufferSize && bytesRead > 0)
            {
                content = state.sb.ToString();
                Parent.ProcessServerMessage(state.buffer, 0, bytesRead);
                return;
            }

            if (bytesRead == handler.ReceiveBufferSize)
            {
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
            }
        }

        public byte[] DecompressString(byte[] data)
        {
            using (var compressedStream = new MemoryStream(data))
            using (var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
            using (var resultStream = new MemoryStream())
            {
                zipStream.CopyTo(resultStream);
                return resultStream.ToArray();
            }
        }

        public void Send(string data)
        {
            byte[] byteData = Encoding.ASCII.GetBytes(data);
            Socket.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), Socket);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket handler = (Socket)ar.AsyncState;
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to WebServer.", bytesSent);

                Read();
                Task.Factory.StartNew(() =>
                {
                    Read();
                });
                SendDone.Set();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void Close()
        {
            if (Socket.Connected)
            {
                Socket.Close();
            }

            if (Stream != null)
            {
                Stream.Close();
            }
        }
    }
}
