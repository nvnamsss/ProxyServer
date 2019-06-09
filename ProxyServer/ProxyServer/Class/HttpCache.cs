using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxyServer.Class
{
    public class HttpCache
    {
        public static HttpCache Empty = new HttpCache();
        public byte[] Content { get; set; }
        public string Host { get; set; }
        
        public void SaveFile()
        {
            string path = Utils.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory, "Cache");
            string filename = Host + ".cache";
            path = Path.Combine(path, filename);
            FileStream stream = new FileStream(path, FileMode.CreateNew);
            stream.Write(Content, 0, Content.Length);
            stream.Close();
        }

        static public List<HttpCache> CacheFromDirectory(string path)
        {
            List<HttpCache> caches = new List<HttpCache>();
            if (Directory.Exists(path))
            {
                DirectoryInfo directory = new DirectoryInfo(path);
                FileInfo[] fileInfos = directory.EnumerateFiles().Where(f => f.Extension.Equals(".cache")).ToArray();

                for (int loop = 0; loop < fileInfos.Length; loop++)
                {
                    caches.Add(CacheFromFile(fileInfos[loop].FullName));
                }
            }
            else
            {
                Console.WriteLine("Directory is not exist");
            }

            return caches;
        }

        static public HttpCache CacheFromFile(string path)
        {
            if (File.Exists(path))
            {

                FileStream stream = File.OpenRead(path);
                FileInfo info = new FileInfo(path);
                HttpCache cache = new HttpCache();

                byte[] bytes = new byte[stream.Length];
                stream.Read(bytes, 0, (int)stream.Length);

                cache.Host = info.Name.Replace(info.Extension, "");
                cache.Content = bytes;

                stream.Close();
                return cache;
            }

            return Empty;
        }

        static public bool IsAlreadyCache(string host)
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Cache");
            string file = host + ".cache";
            path = Path.Combine(path, file);

            return File.Exists(path);
        }
    }
}
