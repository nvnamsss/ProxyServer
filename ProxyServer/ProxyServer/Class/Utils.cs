using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxyServer.Class
{
    public class Utils
    {
        public static string GetAddress(string host, int port)
        {
            return host + ":" + port;
        }

    }
}
