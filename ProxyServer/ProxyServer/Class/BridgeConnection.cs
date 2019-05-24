using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProxyServer.Class
{
    public class BridgeConnection
    {
        public static int CAPACITY { get; } = 5242880;
        public delegate void ClientReceivedHandler(object sender, ReceivedEventArgs e);
        public delegate void ServerReceivedHandler(object sender, ReceivedEventArgs e);
        public event ClientReceivedHandler OnClientReceived;
        public event ServerReceivedHandler OnServerReceived;

        public Thread ProcessThread { get; set; }
        public int Timeout { get; set; }
        public string Name { get; set; }
        public Proxy Proxy { get; set; }
        public Socket SocketClient { get; set; }
        public Socket SocketServer { get; set; }

        public bool Side { get; set; } = true;

        private Task TaskReceiveClient { get; set; }
        private Task TaskReceiveServer { get; set; }

        public string Test { get; set; } = "";

        public BridgeConnection()
        {
            
            SocketClient = new Socket(SocketType.Stream, ProtocolType.Tcp);
            SocketServer = new Socket(SocketType.Stream, ProtocolType.Tcp);
            SocketServer.ReceiveBufferSize = CAPACITY;
            SocketClient.ReceiveBufferSize = CAPACITY;
            SocketServer.SendBufferSize = CAPACITY;
            SocketClient.SendBufferSize = CAPACITY;

            Timeout = 0;
            OnClientReceived += ReceivedClientCallback;
            OnServerReceived += ReceivedServerCallback;

            ProcessThread = new Thread(new ThreadStart(Process));
            ProcessThread.Start();
        }

        public BridgeConnection(Socket client)
        {
            SocketClient = client;
            SocketServer = new Socket(SocketType.Stream, ProtocolType.Tcp);
            SocketServer.ReceiveBufferSize = CAPACITY;
            SocketClient.ReceiveBufferSize = CAPACITY;
            SocketServer.SendBufferSize = CAPACITY;
            SocketClient.SendBufferSize = CAPACITY;

            Timeout = 0;
            OnClientReceived += ReceivedClientCallback;
            OnServerReceived += ReceivedServerCallback;

            ProcessThread = new Thread(new ThreadStart(Process));
            ProcessThread.Start();
        }   

        public void Process()
        {
            byte[] bytes = new byte[CAPACITY];
            int total = 0;
            string data = string.Empty;
            if (Side)
            {
                while (true)
                {
                    int receive = SocketClient.Receive(bytes);
                    data += Encoding.ASCII.GetString(bytes, 0, receive);

                    if (receive == 0 || receive < CAPACITY)
                    {
                        break;
                    }
                }

                if (data != string.Empty)
                {
                    Console.WriteLine("Client: " + data);
                    ProcessClientMessage(data);
                }
            }
            else
            {
                Console.WriteLine("Receive server");
                while (true)
                {
                    int receive = SocketServer.Receive(bytes);
                    data += Encoding.ASCII.GetString(bytes, 0, receive);

                    if (receive == 0 || receive < CAPACITY)
                    {
                        break;
                    }
                }

                if (data != string.Empty)
                {
                    Console.WriteLine("Server: " + data);
                    ProcessServerMessage(data);
                }
            }



        }

        
        public void Close()
        {
            SocketClient.Close();
            SocketServer.Close();
        }

        private void OnClientReceivedInvoke(ReceivedEventArgs e)
        {
            ClientReceivedHandler handler = OnClientReceived;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnServerReceivedInvoke(ReceivedEventArgs e)
        {
            OnServerReceived?.Invoke(this, e);
        }

        private void ReceivedClientCallback(object sender, ReceivedEventArgs e)
        {
            ProcessClientMessage(e.Data);
        }

        private void ReceivedServerCallback(object sender, ReceivedEventArgs e)
        {
            Console.WriteLine("Received from server: {0}\n", e.Data);
        }

        private async void ProcessClientMessage(string message)
        {
            HttpMessage httpMessage = new HttpMessage(message);
            httpMessage.Resolve();
            switch (httpMessage.Method)
            {
                case HttpMethod.CONNECT:
                    
                    if (!SocketServer.Connected)
                    {
                        string host = httpMessage.GetHost();
                        int port = httpMessage.GetPort();
                        SocketServer.Connect(host, 80);
                        Thread.Sleep(100);
                    }
                    ResponseConnect();
                    SendGetRequest(httpMessage);
                    break;
                case HttpMethod.GET:
                    if (!SocketServer.Connected)
                    {
                        string host = httpMessage.GetHost();
                        int port = httpMessage.GetPort();
                        SocketServer.Connect(host, 80);
                    }
                    SendGetRequest(message);
                    break;
                case HttpMethod.POST:

                    break;
            }

            //await SendGetRequest(httpMessage);
            Side = false;
            Console.WriteLine("Side " + Side);
        }

        private void ProcessServerMessage(string message)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(message);
            SocketClient.Send(bytes, 0, bytes.Length, SocketFlags.None);
            Side = true;
        }

        private void ResponseConnect()
        {
            string response = "HTTP/1.1 200 OK\r\n";
            SocketClient.Send(Encoding.ASCII.GetBytes(response));
            Console.WriteLine(response);
        }

        private void SendGetRequest(string message)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(message);
            SocketError error; 
            SocketServer.Send(bytes, 0, bytes.Length, SocketFlags.None, out error);
        }

        private async Task SendGetRequest(HttpMessage message)
        {
            string request = "GET " + message.Uri.PathAndQuery + "/ HTTP/1.1\r\n" +
                "Host: " + message.GetHost() + "\r\n" +
                "User-agent: Mozilla/5.0\r\n\r\n";
            SocketServer.Send(Encoding.ASCII.GetBytes(request));
            Console.WriteLine("Send Server: " + Side);
            return;
        }
        private void Send403()
        {
            string response = "HTTP/1.1 403 Forbidden\r\n";
        }
    }
}
