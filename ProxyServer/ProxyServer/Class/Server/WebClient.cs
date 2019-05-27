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
        public static ManualResetEvent allDone = new ManualResetEvent(false);
        public Dictionary<string, string> Headers { get; set; }
        private Socket Socket { get; set; }
        private HttpMessage Message { get; set; }
        private byte[] Bytes { get; set; }
        private Uri Address { get; set; }
        private NetworkStream Stream = null;

        public SWebClient(BridgeConnection parent)
        {
            Headers = new Dictionary<string, string>();
            Socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            Bytes = new byte[CAPACITY];
            Parent = parent;
        }

        public SWebClient(BridgeConnection parent, string address)
        {
            Headers = new Dictionary<string, string>();
            Socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            Bytes = new byte[CAPACITY];
            Address = new Uri(address);
            Socket.Connect(Address.Host, Address.Port);
            Stream = new NetworkStream(Socket);
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

        public void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;

            // Retrieve the state object and the handler socket  
            // from the asynchronous state object.  
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            // Read data from the client socket.   
            int bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                // There  might be more data, so store the data received so far.  
                state.sb.Append(Encoding.ASCII.GetString(
                    state.buffer, 0, bytesRead));

                // Check for end-of-file tag. If it is not there, read   
                // more data.  
                content = state.sb.ToString();
                if (content.IndexOf("<EOF>") > -1)
                {
                    // All the data has been read from the   
                    // client. Display it on the console.  
                    Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",
                        content.Length, content);
                    // Echo the data back to the client.  
                    //Send(handler, content);
                }
                else
                {
                    // Not all data received. Get more.  
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
                }
                Console.WriteLine("Received {0} bytes from webserver", bytesRead);
                Console.WriteLine("Received from webserver: " + Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
                Parent.ProcessServerMessage(state.buffer, 0, bytesRead);
                Parent.ProcessServerMessage(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
                //HttpMessage message = new HttpMessage(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
                //message.Resolve();
                
                //Console.WriteLine("content: " + DecompressString(Encoding.ASCII.GetString(state.buffer, 0, bytesRead)));
            }
            else
            {
                Parent.ProcessServerMessage(content);
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

        public string DecompressString(string data)
        {
            string content = Encoding.ASCII.GetString(DecompressString(Encoding.ASCII.GetBytes(data)));
            return content;
        }

        public void Send(string data)
        {
            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.  
            Socket.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), Socket);
        }

        public void Read()
        {
            StateObject state = new StateObject();
            state.workSocket = Socket;
            SocketError error;
            state.workSocket.BeginReceive(state.buffer, 0, state.buffer.Length, SocketFlags.None, out error, new AsyncCallback(ReadCallback), state);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to WebServer.", bytesSent);

                Read();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void Close()
        {
            //Socket.Shutdown(SocketShutdown.Both);
            Socket.Close();
            allDone.Close();
            if (Stream != null)
            {
                Stream.Close();
            }
        }
    }
}
