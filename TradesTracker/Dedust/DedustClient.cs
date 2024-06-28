using System.Text.Json;
using TonSharp.Core;
using TradesTracker.Converters;
using TradesTracker.Toncenter;

namespace TradesTracker.Dedust
{
    public class DedustClient : IDedustClient
    {
        private readonly HttpClient _httpClient;
        private readonly IToncenterClient _toncenterClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public DedustClient(HttpClient client, IToncenterClient toncenterClient)
        {
            _httpClient = client;
            _toncenterClient = toncenterClient;
            _jsonOptions = new JsonSerializerOptions();
            _jsonOptions.Converters.Add(new UInt128Converter());
        }

        public Task<DedustAsset> GetAssetAsync(Address address)
        {
            return SendGetAsync<DedustAsset>($"jettons/{address.ToRawString()}/metadata");
        }

        public Task<Trade[]> GetTradesAsync(Address poolAddress, int count, UInt128? afterLt = null)
        {
            UriQueryBuilder uri = new($"pools/{poolAddress.ToRawString()}/trades");
            uri.AddParameter("page_size", count.ToString());
            if (afterLt != null)
                uri.AddParameter("after_lt", afterLt.Value.ToString());
            return SendGetAsync<Trade[]>(uri.Build());
        }

        public async Task UpdatePoolAsync(DedustPool pool)
        {
            RunGetMethodResult result = await _toncenterClient.RunGetMethodAsync(pool.Address, "get_reserves");
            pool.ReserveLeft = result.ReadUInt();
            pool.ReserveRight = result.ReadUInt();
        }

        private async Task<T> SendGetAsync<T>(string url)
        {
            HttpResponseMessage response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return JsonSerializer.Deserialize<T>(response.Content.ReadAsStream(), _jsonOptions)
                ?? throw new InvalidOperationException("No object was deserialized");
        }
    }
}
