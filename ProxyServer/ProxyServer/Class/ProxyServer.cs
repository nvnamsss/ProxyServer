using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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

        private ManualResetEvent Stopping { get; set; }
        private Thread ListenThread { get; set; }

        public Proxy()
        {
            string address = Utils.GetAddress(ConstantProperty.PROXY_HOST, ConstantProperty.PROXY_PORT);
            //IPAddress = IPAddress.Parse(ConstantProperty.PROXY_HOST);
            //IPEndPoint = new IPEndPoint(IPAddress, ConstantProperty.PROXY_PORT);
           
            SocketListener = new Socket(SocketType.Stream, ProtocolType.Tcp);
            SocketSender = new Socket(SocketType.Stream, ProtocolType.Tcp);
            IPAddress = IPAddress.Parse("127.0.0.1");
            IPEndPoint = new IPEndPoint(IPAddress, ConstantProperty.PROXY_PORT);
            Connections = new List<BridgeConnection>(100);
            BlackList = new List<string>();
            Run();
        }

        public void StartListen()
        {
            SocketListener.Bind(IPEndPoint);
            SocketListener.Listen(ConstantProperty.BACKLOG);
            Stopping = new ManualResetEvent(false);
            ListenThread = new Thread(new ThreadStart(AcceptConnection));
            ListenThread.Start();
        }

        public void AcceptConnection()
        {
            try
            {
                while (!Stopping.WaitOne(0))
                {
                    Socket handler = SocketListener.Accept();
                  
                    lock (Connections)
                    {
                        Connections.Add(new BridgeConnection(handler));
                        Connections[Connections.Count - 1].Name = Connections.Count.ToString();
                        Connections[Connections.Count - 1].Parent = this;
                    }
                }
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

        public void Remove(string name)
        {
            for (int loop = 0; loop < Connections.Count; loop++)
            {
                if (Connections[loop].Name.Equals(name))
                {
                    Connections.RemoveAt(loop);
                }
            }
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

            if (BlackList.Contains(host))
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

        }

        public async void Run()
        {
            //while (true)
            //{
            //    foreach (BridgeConnection connection in Connections)
            //    {
            //        connection.ReceiveClient();
            //        connection.ReceiveServer();
            //    }
            //    await Utils.Delay(100);
            //}
            
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
