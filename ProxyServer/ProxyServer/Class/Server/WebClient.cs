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
        public string Content { get; set; }
        public bool IsCache { get; set; }
        private Socket Socket { get; set; }
        private HttpMessage Message { get; set; }
        private List<byte> Bytes { get; set; }
        public Uri Address { get; set; }

        public SWebClient(BridgeConnection parent)
        {
            Socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            Bytes = new List<byte>();

            Socket.ReceiveBufferSize = 262140;
            Socket.SendBufferSize = 262140;
            Parent = parent;
        }

        public SWebClient(BridgeConnection parent, string address)
        {
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
            }
        }

        public void Connect(Uri uri)
        {
            Socket.Connect(uri.Host, 80);
            Address = uri;
        }

        public void Receive()
        {
            StateObject state = new StateObject();
            state.workSocket = Socket;
            SocketError error;
            state.workSocket.BeginReceive(state.buffer, 0, state.buffer.Length, SocketFlags.None, out error, new AsyncCallback(ReceiveCallback), state);
        }

        public void ReceiveCallback(IAsyncResult ar)
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

            try
            {
                if (bytesRead < handler.ReceiveBufferSize && bytesRead > 0)
                {
                    content = state.sb.ToString();
                    Parent.ProcessServerMessage(state.buffer, 0, bytesRead);
                    return;
                }
            }
            catch (ObjectDisposedException ode)
            {
                Parent.Close();
                return;
            }

            try
            {
                if (bytesRead == handler.ReceiveBufferSize)
                {
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
                }
            }
            catch (ObjectDisposedException ode)
            {
                Parent.Close();
                return;
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

                Receive();
                Task.Factory.StartNew(() =>
                {
                    Receive();
                });
                SendDone.Set();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public int Remaining()
        {
            try
            {
                return Socket.Available;
            }
            catch (ObjectDisposedException ode)
            {
                return 0;
            }
        }

        public void Close()
        {
            if (Socket.Connected)
            {
                Socket.Close();
            }
        }
    }
}
