using System.Text.Json.Serialization;

namespace TradesTracker.Dedust
{
    public class Trade
    {
        [JsonPropertyName("sender")]
        public string Sender { get; init; } = string.Empty;

        [JsonPropertyName("assetIn")]
        public required TradeAsset AssetIn { get; init; }
        [JsonPropertyName("assetOut")]
        public required TradeAsset AssetOut { get; init; }

        [JsonPropertyName("amountIn")]
        public required UInt128 AmountIn { get; init; }

        [JsonPropertyName("amountOut")]
        public required UInt128 AmountOut { get; init; }

        [JsonPropertyName("lt")]
        public required UInt128 Lt { get; init; }
    }
}
