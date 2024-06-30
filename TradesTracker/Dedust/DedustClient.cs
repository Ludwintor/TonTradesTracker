using System.Text.Json;
using TonSharp.Core;
using TradesTracker.Converters;
using TradesTracker.Toncenter;

namespace TradesTracker.Dedust
{
    public class DedustClient : IDedustClient
    {
        private static readonly Address _factoryAddress = Address.ParseRaw("0:5f0564fb5f604783db57031ce1cf668a88d4d4d6da6de4db222b4b920d6fd800");
        private static readonly string _tonAssetBoc = Cell.Begin().StoreUInt(0, 4).End().ToBoc().ToBase64String();

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

        public async Task<Address> GetTonPoolAddress(Address jetton)
        {
            Buffer256 hash = jetton.Hash;
            Cell jettonAsset = Cell.Begin()
                .StoreUInt(1, 4) // jetton asset flag
                .StoreUInt(jetton.Workchain, 8) // workchain
                .StoreBytes(hash) // 256 bit address hash
                .End();
            StackEntry[] stack = [
                new StackEntry { Type = "num", Value = "0"}, // pool type
                new StackEntry { Type = "slice", Value = _tonAssetBoc}, // ton asset
                new StackEntry { Type = "slice", Value = jettonAsset.ToBoc().ToBase64String()} // jetton asset
            ];
            RunGetMethodResult result = await _toncenterClient.RunGetMethodAsync(_factoryAddress, "get_pool_address", stack);
            return result.ReadAddress();
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
