using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxyServer.Class
{
    public class HttpMessage
    {
        private string Message { get; set; }
        public string Command { get; set; }
        public string Version { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public HttpMethod Method { get; set; }

        public HttpMessage(string message)
        {
            Message = message;
            Version = "";
            Method = HttpMethod.Connect;
        }

        public void Resolve()
        {
            if (Message != "")
            {
                HttpMethod method;
                Enum.TryParse(Message.Substring(0, Message.IndexOf(" ")), true, out method);
                Method = method;

                int hostIndex = Message.IndexOf("Host:");
            }

        }

        public override string ToString()
        {
            string content = "HTTP Message" + Environment.NewLine +
                "Host: " + Host + Environment.NewLine +
                "Method: " + Method + Environment.NewLine;
            return content;
        }
    }

    public enum HttpMethod
    {
        Connect,
        Post,
        Get
    }
}
