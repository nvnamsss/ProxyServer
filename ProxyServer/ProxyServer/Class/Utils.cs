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

        public static Task Delay(double milliseconds)
        {
            //this.log("Delay", null, "DEBUG");

            try
            {
                var tcs = new TaskCompletionSource<bool>();
                System.Timers.Timer timer = new System.Timers.Timer();
                timer.Elapsed += (obj, args) =>
                {
                    tcs.TrySetResult(true);
                };
                timer.Interval = milliseconds;
                timer.AutoReset = false;
                timer.Start();
                return tcs.Task;
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Exception in Delay: " + ex.Message);
                //this.log(ex.Message, ex.StackTrace);
                throw ex;
            }
        }
    }
}
