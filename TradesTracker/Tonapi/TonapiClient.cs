using System.Text.Json;
using TonSharp.Core;

namespace TradesTracker.Tonapi
{
    public sealed class TonapiClient : ITonapiClient
    {
        private readonly HttpClient _client;

        public TonapiClient(HttpClient client)
        {
            _client = client;
        }

        public async Task<TokenRates> GetTonUsdRate()
        {
            JsonElement json = await GetRatesJson("rates?tokens=ton&currencies=usd");
            JsonElement tonJson = json.GetProperty("TON");
            return tonJson.Deserialize<TokenRates>()
                ?? throw new InvalidOperationException("No object was deserialized");
        }

        public async Task<TokenRates> GetJettonRates(Address address)
        {
            string rawAddress = address.ToRawString();
            JsonElement json = await GetRatesJson($"rates?tokens={rawAddress}&currencies=ton,usd");
            JsonElement jettonJson = json.GetProperty(rawAddress);
            return jettonJson.Deserialize<TokenRates>()
                ?? throw new InvalidOperationException("No object was deserialized");
        }

        private async Task<JsonElement> GetRatesJson(string url)
        {
            HttpResponseMessage response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            JsonElement json = JsonDocument.Parse(response.Content.ReadAsStream()).RootElement;
            return json.GetProperty("rates");
        }
    }
}
