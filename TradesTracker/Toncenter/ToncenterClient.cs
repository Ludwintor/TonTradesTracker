using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using TonSharp.Core;

namespace TradesTracker.Toncenter
{
    public sealed class ToncenterClient : IToncenterClient
    {
        private readonly HttpClient _client;

        public ToncenterClient(HttpClient client)
        {
            _client = client;
        }

        public async Task<RunGetMethodResult> RunGetMethodAsync(Address address, string method, StackEntry[]? stack = null)
        {
            RunGetMethodBody body = new()
            {
                Address = address.ToRawString(),
                Method = method,
                Stack = stack ?? []
            };
            JsonContent content = JsonContent.Create(body);
            HttpResponseMessage response = await _client.PostAsync("runGetMethod", content);
            response.EnsureSuccessStatusCode();
            RunGetMethodResult? result = JsonSerializer.Deserialize<RunGetMethodResult>(response.Content.ReadAsStream());
            if (result == null || result.ExitCode != 0)
                throw new HttpRequestException($"Bad exit code: {result?.ExitCode.ToString() ?? "NULL"}");
            return result;
        }

        private sealed class RunGetMethodBody
        {
            [JsonPropertyName("address")]
            public required string Address { get; init; }

            [JsonPropertyName("method")]
            public required string Method { get; init; }

            [JsonPropertyName("stack")]
            public StackEntry[] Stack { get; init; } = [];
        }
    }
}
