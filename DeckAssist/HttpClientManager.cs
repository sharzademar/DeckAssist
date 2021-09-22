using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace DeckAssist
{
    static class HttpClientManager
    {
        static readonly HttpClient client = new HttpClient();

        public static void UseProperTLS()
        {
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
        }

        public static async Task<HttpResponseMessage> GetRequest(string request)
        {
            return await client.GetAsync(request);
        }

        public static async Task<string> GetResponseContent(HttpResponseMessage message)
        {
            return await message.Content.ReadAsStringAsync();
        }
    }
}
