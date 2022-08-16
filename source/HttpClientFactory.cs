using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Extras
{
    public class HttpClientFactory
    {
        private static object _lock = 0;
        private static HttpClient client;
        private static DateTime lastClientCreated = DateTime.Now;
        private static TimeSpan timeout = TimeSpan.FromMinutes(1);

        public static HttpClient GetClient()
        {
            lock (_lock)
            {
                if ((DateTime.Now - lastClientCreated) > timeout)
                {
                    client?.Dispose();
                    client = null;
                }
                if (client == null)
                {
                    client = new HttpClient();
                    lastClientCreated = DateTime.Now;
                }
                return client;
            }
        }
    }
}
