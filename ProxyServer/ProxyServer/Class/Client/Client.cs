using ProxyServer.AsyncSocket.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProxyServer.Class
{
    public class Client
    {
        public BridgeConnection Parent { get; set; }
        public Socket Socket { get; set; }
        private ManualResetEvent ConnectDone = new ManualResetEvent(false);
        private ManualResetEvent SendDone = new ManualResetEvent(false);
        private ManualResetEvent ReceiveDone = new ManualResetEvent(false);
        private string Response = string.Empty;
        private int Length = 0;
        private int Sended = 0;
        private byte[] Buffer { get; set; }

        public Client (BridgeConnection parent)
        {
            Parent = parent;
            Socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            Socket.ReceiveBufferSize = 262140;
            Socket.SendBufferSize = 262140;
        }

        public Client(BridgeConnection parent, Socket socket)
        {
            Parent = parent;
            Socket = socket;
            Socket.ReceiveBufferSize = 262140;
            Socket.SendBufferSize = 262140;
        }

        public void Receive()
        {
            try
            {
                StateObject state = new StateObject();
                state.workSocket = Socket;
                Socket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                int bytesRead = client.EndReceive(ar);
                string content = string.Empty;

                if (bytesRead > 0)
                {
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
                }

                if (bytesRead < client.ReceiveBufferSize)
                {
                    content = state.sb.ToString();
                    Parent.ProcessClientMessage(content);
                }
                else
                {
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void Send(string data)
        {
            byte[] byteData = Encoding.ASCII.GetBytes(data);
            Socket.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), Socket);
        }

        public void Send(byte[] bytes, int start, int length)
        {
            if (Socket.Connected)
            {
                Socket.BeginSend(bytes, start, length, 0, new AsyncCallback(SendCallback), Socket);
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;
                int bytesSent = client.EndSend(ar);

                Console.WriteLine("Sent {0} bytes to browser.", bytesSent);

                //Parent.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void Close()
        {
            Socket.Close();
            ConnectDone.Close();
            ReceiveDone.Close();
            SendDone.Close();
        }
    }
}
