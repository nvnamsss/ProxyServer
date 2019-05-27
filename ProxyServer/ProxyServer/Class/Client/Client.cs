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


        public Client (BridgeConnection parent)
        {
            Parent = parent;
            Socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        }

        public Client(BridgeConnection parent, Socket socket)
        {
            Parent = parent;
            Socket = socket;
        }

        public void Receive()
        {
            try
            {
                // Create the state object.  
                StateObject state = new StateObject();
                state.workSocket = Socket;

                // Begin receiving the data from the remote device.  
                Socket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
                Console.WriteLine("Yes, indeed");

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
                // Retrieve the state object and the client socket   
                // from the asynchronous state object. 

                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                // Read data from the remote device.  
                int bytesRead = client.EndReceive(ar);
                if (bytesRead == StateObject.BufferSize)
                {
                    // There might be more data, so store the data received so far.  
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                    // Get the rest of the data.  
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReceiveCallback), state);
                }
                else
                {
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                    Console.WriteLine("hi mom");

                    // All the data has arrived; put it in response.  

                    if (state.sb.Length > 1)
                    {
                        Response = state.sb.ToString();
                        Console.WriteLine("Response: " + Response);
                        Parent.ProcessClientMessage(Response);

                    }
                    //Console.WriteLine("Received data from Browser: \r\n", Response);
                    // Signal that all bytes have been received.  
                    ReceiveDone.Set();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void Send(string data)
        {
            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.  
            Socket.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), Socket);
        }

        public void Send(byte[] bytes, int start, int length)
        {
            Socket.BeginSend(bytes, start, length, 0, new AsyncCallback(SendCallback), Socket);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = client.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to browser.", bytesSent);
                
                // Signal that all bytes have been sent.  
                SendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void Close()
        {
            SendDone.Set();
            ConnectDone.Set();
            ReceiveDone.Set();
            Socket.Shutdown(SocketShutdown.Both);
            Socket.Close();
            ConnectDone.Close();
            ReceiveDone.Close();
            SendDone.Close();
        }
    }
}
