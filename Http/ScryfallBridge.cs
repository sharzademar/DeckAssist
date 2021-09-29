using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DeckAssist.Http
{
    /// <summary>
    /// Defines how requests are mode to scryfall
    /// </summary>
    public static class ScryfallBridge
    {
        private const string scryfallExactURI = "https://api.scryfall.com/cards/named?exact=";
        public const string CardBackURI = "https://c1.scryfall.com/file/scryfall-card-backs/normal/59/597b79b3-7d77-4261-871a-60dd17403388.jpg";

        private static int minLimit = 50;
        private static int RateLimit = minLimit;

        /// <summary>
        /// Sets the rate limit with which requests are made to scryfall servers, cannot be set than lower than minLimit
        /// </summary>
        public static void SetRateLimit(int i)
        {
            if (i > minLimit)
                RateLimit = i;
            else
                RateLimit = minLimit;
        }

        /// <summary>
        /// Represents a call to <c>cards/named?exact</c>
        /// </summary>
        /// <param name="name">The exact name of a card. Throws ArgumentException if not found.</param>
        /// <returns>Scrayfall's response to the request</returns>
        /// <exception cref="ArgumentException"></exception>
        public static async Task<string> FindExact(string name)
        {
            //query the server for cards matching the name
            HttpResponseMessage response = await HttpClientManager.GetRequest(scryfallExactURI + name);
            string responseString = await HttpClientManager.GetResponseContent(response);
            
            //rate limit requests per the request of scryfall
            System.Threading.Thread.Sleep(RateLimit);

            //if the response failed
            if (!response.IsSuccessStatusCode)
            {
                throw new ArgumentException
                (
                    String.Format("ERROR - {0} {1} | Card name passed: \"{2}\"",
                                    (int)response.StatusCode, response.ReasonPhrase, name)
                );
            }

            return responseString;
        }
    }
}
