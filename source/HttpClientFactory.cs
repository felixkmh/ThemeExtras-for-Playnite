using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Extras
{
    public class HttpClientFactory
    {
        private static SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        private static HttpClient client;
        private static DateTime lastClientCreated = DateTime.Now;
        private static TimeSpan timeout = TimeSpan.FromSeconds(100);

        public static HttpClient GetClient()
        {
            lock (semaphore)
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

        public static async Task<HttpClient> GetClientAsync()
        {
            await semaphore.WaitAsync();
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
            semaphore.Release();
            return client;
        }
    }
}
