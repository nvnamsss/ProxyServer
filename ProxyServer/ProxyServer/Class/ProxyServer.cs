using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ProxyServer.Class
{
    public class Proxy
    {
        private string BlacklistPath { get; set; }
        public Socket SocketListener { get; set; }
        public Socket SocketSender { get; set;}
        public Socket Handler { get; set; }
        public IPAddress IPAddress { get; set; }
        public IPEndPoint IPEndPoint { get; set; }
        public List<string> BlackList { get; set; }

        public Proxy()
        {
            string address = Utils.GetAddress(ConstantProperty.PROXY_HOST, ConstantProperty.PROXY_PORT);
            //IPAddress = IPAddress.Parse(ConstantProperty.PROXY_HOST);
            //IPEndPoint = new IPEndPoint(IPAddress, ConstantProperty.PROXY_PORT);
            SocketListener = new Socket(SocketType.Stream, ProtocolType.Tcp);
            SocketSender = new Socket(SocketType.Stream, ProtocolType.Tcp);
            IPAddress = IPAddress.Parse("127.0.0.1");
            IPEndPoint = new IPEndPoint(IPAddress, ConstantProperty.PROXY_PORT);
            BlackList = new List<string>();
        }

        public void Start()
        {
            string data = null;
            byte[] bytes;
            try
            {
                SocketListener.Bind(IPEndPoint);
                SocketListener.Listen(ConstantProperty.BACKLOG);

                Handler = SocketListener.Accept();
                Console.WriteLine("A browser connect to Server");   
                while (true)
                {
                    bytes = new byte[1024];
                    int bytesRec = Handler.Receive(bytes);
                    data += Encoding.ASCII.GetString(bytes, 0, bytesRec);

                    Console.WriteLine(Encoding.ASCII.GetString(bytes, 0, bytesRec));
                    //if (data.IndexOf("<EOF>") > -1)
                    //{
                    //    break;
                    //}
                }
                
                

                byte[] msg = Encoding.ASCII.GetBytes(data);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            
        }

        public void Connect(string host, int port)
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry("localhost");
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);
            //SocketSender.Connect(host, port);
            Uri uri = new Uri("https://www.google.com.vn/");
            Console.WriteLine("Host {0} Port {1}", uri.Host, uri.Port);
            SocketSender.Connect(uri.Host, 80);
            
            Console.Write(SocketSender.Connected);
        }

        public string SendRequest(string message)
        {
            string data = null;
            byte[] bytes = Encoding.UTF8.GetBytes(message);
            SocketSender.Send(bytes);

            while (true)
            {
                bytes = new byte[1024];
                int bytesRec = SocketSender.Receive(bytes);
                data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                //
                if (bytesRec > 0)
                {
                    Console.WriteLine(bytesRec);
                    Console.WriteLine(Encoding.ASCII.GetString(bytes, 0, bytesRec));
                }
                
                //Handler.Send(bytes);
                //if (bytesRec <= 0)
                //{
                //    break;
                //}

                
            }

            Console.WriteLine("Text received : {0}", data);

            return data;
        }

        public void Read()
        {
            string data = null;
            byte[] bytes;
            while (true)
            {

            }
        }

        public void Get(string url)
        {
            string request = "GET /meo HTTP/1.1\r\nHost: localhost\r\n\r\n";
            SendRequest(request);
        }

        public void Disconnect()
        {
            if (SocketSender.Connected)
            {
                SocketSender.Disconnect(true);
            }
        }

        private void Cache()
        {

        }

        private bool CheckBlackList()
        {
            bool isBanned = false;

            return isBanned;
        }

        private void GetBlackList()
        {
            BlackList.Clear();

            FileStream stream = File.OpenRead(BlacklistPath);

            using (var streamReader = new StreamReader(stream, Encoding.UTF8))
            {
                string line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    BlackList.Add(line);
                }
            }

            stream.Close();
        }
    }

    static class SocketExtensions
    {
        public static bool IsConnected(this Socket socket)
        {
            try
            {
                return !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
            }
            catch (SocketException e)
            {
                return false;
            }
        }
    }
}
