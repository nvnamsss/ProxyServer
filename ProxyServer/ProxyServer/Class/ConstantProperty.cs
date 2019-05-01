using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxyServer.Class
{
    public static class ConstantProperty
    {
        public static string PROXY_HOST { get; } = "localhost";
        public static string WEBSERVER_HOST { get; } = "localhost";
        public static int PROXY_PORT { get; } = 8888;
        public static int WEBSERVER_PORT { get; } = 8080;
        public static int BACKLOG { get; } = 20;
    }
}
