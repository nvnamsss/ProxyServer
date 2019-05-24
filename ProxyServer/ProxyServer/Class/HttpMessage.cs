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
        public Uri Uri { get; set; }

        public HttpMessage(string message)
        {
            Message = message;
            Version = "";
            Method = HttpMethod.None;
            Headers = new Dictionary<string, string>();
        }

        public void Resolve()
        {
            if (Message != "")
            {
                HttpMethod method;

                string[] lines = Message.Replace("\r", "").Split('\n');
            
                foreach (string line in lines)
                {
                    if (line == string.Empty)
                    {
                        continue;
                    }

                    if (line.Contains("HTTP/"))
                    {
                        string[] split = line.Split(' ');
                        if (Enum.TryParse(split[0], out method))
                        {
                            Method = method;
                        }

                        Uri = new Uri(split[1].Replace("https", "http"));

                        Version = split[2];
                        //Console.WriteLine("Method: " + Method + " Version: " + Version);
                    }
                    else
                    {
                        //Console.WriteLine("Line: " + line);
                        string[] split = line.Replace(" ", string.Empty).Split(':');
                        string key = split[0];
                        string value = string.Empty;

                        for (int loop = 1; loop < split.Length; loop++)
                        {
                            value += split[loop];
                            if (loop != split.Length - 1)
                            {
                                value += ":";
                            }
                        }

                        //Console.WriteLine("Key: " + split[0] + " Value: " + value);

                        Headers.Add(key, value);
                    }

                }
            }

        }

        public string GetHost()
        {
            return Headers["Host"].Split(':')[0];
        }

        public int GetPort()
        {
            return 443;
            //return int.Parse(Headers["Host"].Split(':')[1]);
        }

        public override string ToString()
        {
            string content = "HTTP Message" + Environment.NewLine +
                "Method: " + Method + Environment.NewLine;
            return content;
        }
    }

    public enum HttpMethod
    {
        CONNECT,
        POST,
        GET,
        None
    }
}
