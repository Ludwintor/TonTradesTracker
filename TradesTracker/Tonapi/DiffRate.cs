using System.Text.Json.Serialization;

namespace TradesTracker.Tonapi
{
    public sealed class DiffRate
    {
        [JsonPropertyName("TON")]
        public string Ton { get; init; } = "0.00%";

        [JsonPropertyName("USD")]
        public string Usd { get; init; } = "0.00%";
    }
}
