using System.Text.Json.Serialization;

namespace TradesTracker.Dedust
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum AssetType
    {
        [JsonPropertyName("native")] Native,
        [JsonPropertyName("jetton")] Jetton
    }
}
