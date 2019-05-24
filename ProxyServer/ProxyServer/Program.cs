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
            //string request = "CONNECT  HTTP/1.1\r\n" +
            //    "Host: www.google.com.vn:443\r\n" +
            //    //"Proxy-Connection: keep-alive\r\n" +
            //    "User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/73.0.3683.103 Safari/537.36\r\n\r\n";
            string request = "GET http://www.google.com/search?source=hp&ei=vTHlXO3tHofchwPQwY3IBg&q=ds&oq=ds&gs_l=psy-ab.12..35i39j0i67l2j0j0i131l2j0j0i131j0j0i131.96206.96298..96726...5.0..0.135.351.0j3......0....1..gws-wiz.....6.L7drwxthGy0 HTTP/1.1\r\n" +
                "Host: www.google.com\r\n" +
                "User-agent: Mozilla/5.0\r\n\r\n";

            proxy.StartListen();
            //Socket socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            //Uri uri = new Uri("http://www.google.com/");
            //socket.Connect(uri.Host, 80);
            //socket.Send(Encoding.ASCII.GetBytes(request));
            //string response = string.Empty;
            //byte[] bytes = new byte[102400];
            //while (true)
            //{
            //    int receiv = socket.Receive(bytes, 0, 102400, SocketFlags.None);
            //    response += Encoding.ASCII.GetString(bytes, 0, receiv);

            //    Thread.Sleep(100);
            //    if (receiv == 0 || receiv < 102400)
            //        break;
            //}

            //Console.WriteLine(response);
            byte[] bytes = new byte[1024000];
            WebClient client = new WebClient();
            Uri uri = new Uri("http://www.google.com/search?ei=_GLmXIiOO5nAoATE6ZigCA&q=what+is+makefile&oq=what+is+makefile&gs_l=psy-ab.3..0i71l8.0.0..482691...0.0..0.0.0.......0......gws-wiz.LnDkgmIj8lQ");
            Console.WriteLine(uri.PathAndQuery);
            Stream stream = client.OpenRead(uri);
            int receiv = stream.Read(bytes, 0, 1024000);
            Console.WriteLine(receiv);
            Console.WriteLine(Encoding.ASCII.GetString(bytes, 0, receiv));
            Console.ReadKey();
        }

       
    }   
}
