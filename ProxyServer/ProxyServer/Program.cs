using ProxyServer.Class;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProxyServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Proxy proxy = new Proxy();
            proxy.StartListen();

            //string request = "CONNECT  HTTP/1.1\r\n" +
            //    "Host: www.google.com.vn:443\r\n" +
            //    //"Proxy-Connection: keep-alive\r\n" +
            //    "User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/73.0.3683.103 Safari/537.36\r\n\r\n";
            //string request = "GET / HTTP/1.0\r\n" +
            //    "Host: soha.vn\r\n" +
            //    "User-agent: Mozilla/5.0\r\n\r\n";


            //Socket socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            //Uri uri = new Uri("http://soha.vn");
            //socket.Connect(uri.Host, 80);
            //socket.Send(Encoding.ASCII.GetBytes(request));
            //string response = string.Empty;
            //byte[] bytes = new byte[102400];
            //while (true)
            //{
            //    int receiv = socket.Receive(bytes, 0, 102400, SocketFlags.None);
            //    response += Encoding.ASCII.GetString(bytes, 0, receiv);

            //    Thread.Sleep(100);
            //    if (receiv== 0)
            //        break;
            //}

            //Console.WriteLine(response);


            //byte[] bytes = new byte[1024000];
            //WebClient client = new WebClient();
            //Uri uri = new Uri("http://www.soha.vn/");
            //string response = "";
            //Console.WriteLine(uri.PathAndQuery);
            //Stream stream = client.OpenRead(uri);
            //while (true)
            //{
            //    int receiv = stream.Read(bytes, 0, 1024000);
            //    response += Encoding.ASCII.GetString(bytes, 0, receiv);

            //    Thread.Sleep(100);
            //    if (receiv == 0)
            //        break;
            //}
            //Console.WriteLine(response);
            Console.ReadKey();
        }

       
    }   
}
