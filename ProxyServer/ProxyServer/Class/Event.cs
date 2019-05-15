using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxyServer.Class
{
    public class ReceivedEventArgs
    {
        public string Data { get; }
        public ReceivedEventArgs()
        {
            Data = "";
        }

        public ReceivedEventArgs(string data)
        {
            Data = data;
        }
    }

}
