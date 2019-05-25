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

        public Proxy Parent { get; set; }

        public Thread ProcessThread { get; set; }
        public int Timeout { get; set; }
        public string Name { get; set; }
        public Proxy Proxy { get; set; }
        public Client SocketClient { get; set; }
        public SWebClient WebClient { get; set; }

        public bool Side { get; set; } = true;

        public BridgeConnection()
        {
            SocketClient = new Client(this);

            WebClient = new SWebClient(this);
            Timeout = 0;

            //ProcessThread = new Thread(new ThreadStart(Process));
            //ProcessThread.Start();

            Process();
        }

        public BridgeConnection(Socket client)
        {
            SocketClient = new Client(this, client);

            WebClient = new SWebClient(this);

            Timeout = 0;

            //ProcessThread = new Thread(new ThreadStart(Process));
            //ProcessThread.Start();

            Process();
        }   

        public void Process()
        {
            byte[] bytes = new byte[CAPACITY];
            int total = 0;
            string data = string.Empty;

            if (Side)
            {
                Console.WriteLine("Receive client");
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
            WebClient.Close();
            SocketClient.Close();
            Parent.Remove(Name);
        }

        private void ReceivedClientCallback(object sender, ReceivedEventArgs e)
        {
            ProcessClientMessage(e.Data);
        }

        private void ReceivedServerCallback(object sender, ReceivedEventArgs e)
        {
            Console.WriteLine("Received from server: {0}\n", e.Data);
        }

        public void ProcessClientMessage(string message)
        {
            HttpMessage httpMessage = new HttpMessage(message);
            httpMessage.Resolve();
            switch (httpMessage.Method)
            {
                case HttpMethod.CONNECT:
                    Close();
                    break;
                case HttpMethod.GET:
                    ResponseConnect();
                    WebClient.Connect(httpMessage.Uri);
                    SendGetRequest(message);
                    break;
                case HttpMethod.POST:

                    break;
            }

        }

        public void ProcessServerMessage(string message)
        {
            //byte[] bytes = Encoding.ASCII.GetBytes(message);
            SocketClient.Send(message);
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
            string response = "HTTP/1.1 403 For bidden\r\n";
            SocketClient.Send(response);
        }
    }
}
