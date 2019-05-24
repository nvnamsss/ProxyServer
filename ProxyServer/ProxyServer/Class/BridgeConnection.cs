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

        public Thread ProcessThread { get; set; }
        public int Timeout { get; set; }
        public string Name { get; set; }
        public Proxy Proxy { get; set; }
        public Client SocketClient { get; set; }
        public SWebClient WebClient { get; set; }

        public bool Side { get; set; } = true;

        public BridgeConnection()
        {
            SocketClient = new Client();

            WebClient = new SWebClient();
            Timeout = 0;

            ProcessThread = new Thread(new ThreadStart(Process));
            ProcessThread.Start();
        }

        public BridgeConnection(Socket client)
        {
            SocketClient = new Client();
            SocketClient.Socket = client;

            Timeout = 0;

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
                SocketClient.Receive();
            }
            else
            {
                Console.WriteLine("Receive server");

                WebClient.Read();
            }
        }   

        
        public void Close()
        {
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
                    WebClient.Connect(httpMessage.Uri);
                    ResponseConnect();
                    break;
                case HttpMethod.GET:
                    WebClient.Connect(httpMessage.Uri);
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
        }

        private void ResponseConnect()
        {
            string response = "HTTP/1.1 200 OK\r\n";
            SocketClient.Send(response);
            Console.WriteLine(response);
        }

        private void SendGetRequest(string message)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(message);
            SocketError error; 
            WebClient.Send(message);
        }

        private void SendGetRequest(HttpMessage message)
        {
            string request = "GET " + message.Uri.PathAndQuery + "/ HTTP/1.1\r\n" +
                "Host: " + message.GetHost() + "\r\n" +
                "User-agent: Mozilla/5.0\r\n\r\n";
            WebClient.Send(request);
            return;
        }
        private void Send403()
        {
            string response = "HTTP/1.1 403 Forbidden\r\n";
        }
    }
}
