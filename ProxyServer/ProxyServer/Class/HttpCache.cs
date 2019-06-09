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
        public string Query { get; set; }
        public string Header { get; set; }

        public void SaveFile()
        {
            string pathCache = Utils.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory, "Cache");
            string fileCache = Host + ".cache";
            string fileHeader = Host + ".hdr";
            fileCache = Path.Combine(pathCache, fileCache);
            fileHeader = Path.Combine(pathCache, fileHeader);

            try
            {
                FileStream streamCache = new FileStream(fileCache, FileMode.CreateNew);

                streamCache.Write(Content, 0, Content.Length);

                File.WriteAllText(fileHeader, Header);

                streamCache.Close();
            }
            catch
            {

            }
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

                string header = File.ReadAllText(path.Replace(".cache", ".hdr"));
                byte[] bytes = new byte[stream.Length];
                stream.Read(bytes, 0, (int)stream.Length);

                cache.Host = info.Name.Replace(info.Extension, "");
                cache.Content = bytes;
                cache.Header = header;

                HttpMessage message = new HttpMessage(Encoding.UTF8.GetString(bytes));
                message.Resolve();

                cache.Query = message.Query;

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
