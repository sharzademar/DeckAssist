using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace DeckAssist.Http
{
    /// <summary>
    /// Facilitates HTTPS requests
    /// </summary>
    internal static class HttpClientManager
    {
        private static readonly HttpClient client = new HttpClient();

        /// <summary>
        /// Send a GET request to the specified URI
        /// </summary>
        /// <param name="request">The URI of the request</param>
        /// <returns>The response message returned by the server</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="HttpRequestException"></exception>
        public static async Task<HttpResponseMessage> GetRequest(string request)
        {
            return await client.GetAsync(request);
        }

        /// <summary>
        /// Extract the message content from a server response to an HTTP request
        /// </summary>
        /// <param name="message">The response message returned by the server</param>
        /// <returns>The message content as a <c>string</c></returns>
        public static async Task<string> GetResponseContent(HttpResponseMessage message)
        {
            return await message.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Ensures the proper security protocol is being used for https
        /// </summary>
        public static void UseProperTLS()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
        }
    }
}