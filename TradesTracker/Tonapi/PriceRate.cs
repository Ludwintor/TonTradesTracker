using System.Text.Json.Serialization;

namespace TradesTracker.Tonapi
{
    public sealed class PriceRate
    {
        [JsonPropertyName("TON")]
        public double Ton { get; init; } = 0;

        [JsonPropertyName("USD")]
        public double Usd { get; init; } = 0;
    }
}
