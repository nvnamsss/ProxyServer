using ProxyServer.Class;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxyServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Proxy proxy = new Proxy();
            string request = "CONNECT  HTTP/1.1\r\n" +
                "Host: www.google.com.vn:443\r\n" +
                //"Proxy-Connection: keep-alive\r\n" +
                "User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/73.0.3683.103 Safari/537.36\r\n\r\n";
 
            //proxy.Start();
            //request = "GET HTTP/1.0";
            proxy.Connect(ConstantProperty.WEBSERVER_HOST, ConstantProperty.WEBSERVER_PORT);
            proxy.SendRequest(request);
            Console.ReadKey();
        }
    }   
}
