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
        public delegate void ClientReceivedHandler(object sender, ReceivedEventArgs e);
        public delegate void ServerReceivedHandler(object sender, ReceivedEventArgs e);
        public event ClientReceivedHandler OnClientReceived;
        public event ServerReceivedHandler OnServerReceived;

        public int Timeout { get; set; }
        public string Name { get; set; }
        public Proxy Proxy { get; set; }
        public Socket SocketClient { get; set; }
        public Socket SocketServer { get; set; }

        public bool IsReceiveClient { get; set; }
        public bool IsReceiveServer { get; set; }

        private Task TaskReceiveClient { get; set; }
        private Task TaskReceiveServer { get; set; }

        public string Test { get; set; } = "";

        public BridgeConnection()
        {
            
            SocketClient = new Socket(SocketType.Stream, ProtocolType.Tcp);
            SocketServer = new Socket(SocketType.Stream, ProtocolType.Tcp);

            IsReceiveClient = false;
            IsReceiveClient = false;

            Timeout = 0;
            OnClientReceived += ReceivedClientCallback;
            OnServerReceived += ReceivedServerCallback;

            StartReceiveClient();
            StartReceiveServer();
        }

        public BridgeConnection(Socket client)
        {
            SocketClient = client;
            SocketServer = new Socket(SocketType.Stream, ProtocolType.Tcp);

            IsReceiveClient = false;
            IsReceiveClient = false;

            OnClientReceived += ReceivedClientCallback;
            OnServerReceived += ReceivedServerCallback;

            StartReceiveClient();
            StartReceiveServer();
        }   


        public void SendClient(string request )
        {
            byte[] bytes = Encoding.UTF8.GetBytes(request);

            if (SocketClient.Connected)
            {
                SocketClient.Send(bytes);
            }
            else
            {
                Console.WriteLine("Client is not connected to Proxy Server");
            }
        }

        public void SendServer(string request)
        {
            
            byte[] bytes = Encoding.UTF8.GetBytes(request);
            SocketServer.Send(bytes);
            Console.Write("Sending...");
        }

        public string ReceiveClient()
        {
            string data = null;
            int totalRec = 0;
            byte[] bytes;

            if (!SocketClient.Connected)
            {
                //Console.WriteLine("Client is not connected to Proxy Server");
                return "";
            }

            IsReceiveClient = true;
            while (true)
            {
                bytes = new byte[ConstantProperty.BUFFER_SIZE];
                int bytesRec = SocketClient.Receive(bytes);
                totalRec += bytesRec;

                data += Encoding.UTF8.GetString(bytes, 0, bytesRec);
          
                if (bytesRec < ConstantProperty.BUFFER_SIZE)
                {
                    break;
                }
                Thread.Sleep(200);
            }
            Console.WriteLine("Received from client {0}\r\n{1}", Name, data);
            IsReceiveClient = false;
            if (totalRec == 0)
            {
                Send403();
                return "";
            }

            OnClientReceivedInvoke(new ReceivedEventArgs(data));

            return data;
        }

        public string ReceiveServer()
        {
            string data = null;
            byte[] bytes;
            int totalRec = 0;

            if (!SocketServer.Connected)
            {
                //Console.WriteLine("Server is not connected to Proxy Server");
                return "";
            }
            IsReceiveServer = true;
            while (true)
            {
                bytes = new byte[ConstantProperty.BUFFER_SIZE];
                int bytesRec = SocketServer.Receive(bytes);
                totalRec += bytesRec;

                data += Encoding.UTF8.GetString(bytes, 0, bytesRec);

                if (SocketServer.Available == 0)
                {
                    //Console.WriteLine("Break");
                    break;
                }
                Thread.Sleep(200);
            }
            IsReceiveServer = false;
            //Console.WriteLine("Received from server: \n{0}", data);

            if (totalRec == 0)
            {
                return "";
            }

            //SocketServer.Disconnect(true);
            OnServerReceivedInvoke(new ReceivedEventArgs(data));

            return data;
        }

        public void ConnectServer(string host, int port)
        {
            try
            {
                //IPHostEntry hostInfo = Dns.GetHostEntry(host);
                if (!SocketServer.Connected)
                {
                    //SocketServer.Disconnect(true);
                    SocketServer.Connect("www.google.com", 80);
                }
                

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
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

        private void StartReceiveClient()
        {
            Console.WriteLine("Start receive Client task");
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = 200;
            timer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e) =>
            {
                //Console.WriteLine("From {0}", Name);
                if (!IsReceiveClient)
                {
                    ReceiveClient();
                }
                
            };

            timer.Start();
        }

        private void StartReceiveServer()
        {
            Console.WriteLine("Start receive Server task");
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = 200;
            timer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e) =>
            {
                if (!IsReceiveServer)
                {
                    ReceiveServer();
                }
                
            };

            timer.Start();
        }

        private void ReceivedClientCallback(object sender, ReceivedEventArgs e)
        {
            //HttpMessage message = new HttpMessage(e.Data);
            //SendServer(e.Data);
            ProcessClientMessage(e.Data);
        }

        private void ReceivedServerCallback(object sender, ReceivedEventArgs e)
        {
            Console.WriteLine("Received from server: {0}\n", e.Data);
            //HttpMessage message = new HttpMessage(e.Data);
            SendClient(e.Data);
        }

        private void ProcessClientMessage(string message)
        {
            HttpMessage httpMessage = new HttpMessage(message);
            httpMessage.Resolve();
            switch (httpMessage.Method)
            {
                case HttpMethod.Connect:
                    //SocketServer.Connect(httpMessage.Host, 80);
                    ConnectServer(httpMessage.Host, 80);
                    ResponseConnect(httpMessage.Host);
                    //SendGetRequest(httpMessage.Host);
                    break;
                case HttpMethod.Get:
                    if (!Proxy.CheckBlackList(httpMessage.Host))
                    {
                        SendClient(message);
                    }
                    
                    break;
                case HttpMethod.Post:
                    if (!Proxy.CheckBlackList(httpMessage.Host))
                    {
                        SendClient(message);
                    }
                    break;
            }
        }

        private void ResponseConnect(string host)
        {
            Console.WriteLine("Response connect request to {0}", host);
            //string request = "GET /" + host + "HTTP/1.1\r\n" +
            //    "Host: www.google.com\r\n" +
            //    "User-agent: Mozilla/5.0\r\n" +
            //    "Connection: close\r\n" +
            //    "Accept-language:fr\r\n\r\n";
            string response = "HTTP/1.1 200 OK\r\n";

            SendClient(response);
        }

        private void SendGetRequest(string host)
        {
            string request = "GET / HTTP/1.1\r\n" +
                "Host: " + host + "\r\n" +
                "User-agent: Mozilla/5.0\r\n" +
                "Connection: close\r\n" +
                "Accept-language:fr\r\n\r\n";
        }

        private void Send403()
        {
            string response = "HTTP/1.1 403 Forbidden\r\n";
            SendClient(response);
        }
    }
}
