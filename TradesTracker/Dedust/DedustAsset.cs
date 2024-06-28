using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace TradesTracker.Dedust
{
    public sealed class DedustAsset
    {
        public static DedustAsset Ton { get; } = new("TON", 9);

        [JsonPropertyName("symbol")]
        public string Symbol { get; init; }

        [JsonPropertyName("decimals")]
        public int Decimals { get; init; }

        public DedustAsset(string symbol, int decimals)
        {
            Symbol = symbol;
            Decimals = decimals;
        }
    }
}
