using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ProxyServer.Class
{
    public class HttpRespone
    {
        public string Version { get; set; }
        public HttpResponseMessage Response { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public string Content { get; set; }

        public string Message { get; set; }

        public HttpRespone()
        {
            Headers = new Dictionary<string, string>();
        }

        public HttpRespone(string message)
        {
            Headers = new Dictionary<string, string>();
            Message = message;
        }

        public void Resolve()
        {
            if (Message != string.Empty)
            {
                try
                {
                    string[] lines = Message.Replace("\r", "").Split('\n');

                    Content = Message.Substring(Message.IndexOf("\r\n\r\n")).Replace("\r\n\r\n", "");

                    string firstLine = lines[0];

                    string[] datas = firstLine.Split(' ');
                    Version = datas.Length > 0 ? datas[0] : string.Empty;
                    StatusCode = (HttpStatusCode)(int.Parse(datas[1]));
                }
                catch
                {

                }
            }
        }
    }

}
