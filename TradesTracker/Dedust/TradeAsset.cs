using System.Text.Json.Serialization;

namespace TradesTracker.Dedust
{
    public class TradeAsset
    {
        [JsonPropertyName("type")]
        public required AssetType Type { get; init; }

        [JsonPropertyName("address")]
        public string Address { get; init; } = string.Empty;
    }
}
