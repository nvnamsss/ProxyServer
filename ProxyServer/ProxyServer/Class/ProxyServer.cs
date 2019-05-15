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
        public List<BridgeConnection> Connections { get; set; }
        public IPAddress IPAddress { get; set; }
        public IPEndPoint IPEndPoint { get; set; }
        public List<string> BlackList { get; set; }
        public System.Timers.Timer Timer = new System.Timers.Timer();

        public Proxy()
        {
            string address = Utils.GetAddress(ConstantProperty.PROXY_HOST, ConstantProperty.PROXY_PORT);
            //IPAddress = IPAddress.Parse(ConstantProperty.PROXY_HOST);
            //IPEndPoint = new IPEndPoint(IPAddress, ConstantProperty.PROXY_PORT);
           
            SocketListener = new Socket(SocketType.Stream, ProtocolType.Tcp);
            SocketSender = new Socket(SocketType.Stream, ProtocolType.Tcp);
            IPAddress = IPAddress.Parse("127.0.0.1");
            IPEndPoint = new IPEndPoint(IPAddress, ConstantProperty.PROXY_PORT);
            Connections = new List<BridgeConnection>(10);
            BlackList = new List<string>();

            Run();
        }

        public void Listen()
        {
            string data = null;
            byte[] bytes;
            try
            {
                SocketListener.Bind(IPEndPoint);
                SocketListener.Listen(ConstantProperty.BACKLOG);

                Listen:
               
                Socket handler = SocketListener.Accept();
                Console.WriteLine("New Connection");
                lock (Connections)
                {
                    Connections.Add(new BridgeConnection(handler));
                    Connections[Connections.Count - 1].Name = Connections.Count.ToString();
                }
                
                //Connections[0].SocketClient = handler;
                goto Listen;

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            
        }

        public void Connect(string host, int port)
        {
            SocketSender.Connect(host, port);
            Console.Write(SocketSender.Connected);
        }

        public void DisconnectAll()
        {
            for (int loop = 0; loop < Connections.Count; loop++)
            {

            }
            if (SocketSender.Connected)
            {
                SocketSender.Disconnect(true);
            }
        }

        private void Cache()
        {

        }

        public bool CheckBlackList(string host)
        {
            bool isBanned = false;
            //Console.WriteLine("Checking black list for {0}", host);

            if (host != "www.google.com")
            {
                isBanned = true;
            }

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

        public void Close()
        {
            SocketListener.Close();
            SocketSender.Close();
            for (int loop = 0; loop < Connections.Count; loop++)
            {
                Connections[loop].SocketClient.Close();
                Connections[loop].SocketServer.Close();
            }
        }

        public void Run()
        {
            Timer.Interval = 200;
            Timer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e) =>
            {
                lock (Connections)
                {
                    int length = Connections.Count;
                    for (int loop = 0; loop < length; loop++)
                    {
                        if (!Connections[loop].SocketClient.Connected)
                        {
                            Connections[loop].Timeout += 200;
                        }

                        if (Connections[loop].Timeout > 10000)
                        {
                            Connections.RemoveAt(loop);
                            loop = loop - 1;
                            continue;
                        }

                        string message = Connections[loop].ReceiveClient();
                        if (message.Contains("CONNECT"))
                        {
                            Connections[loop].Timeout = 0;
                            string response = "HTTP/1.1 200 OK\r\n";
                            Console.WriteLine("Response");
                            Connections[loop].SendClient(response);
                        }
                    }
                }
               
            };

            Timer.Start();
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
