using System.Text.Json.Serialization;

namespace TradesTracker.Tonapi
{
    public sealed class TokenRates
    {
        [JsonPropertyName("prices")]
        public required PriceRate Price { get; init; }

        [JsonPropertyName("diff_24h")]
        public required DiffRate DailyDiff { get; init; }

        [JsonPropertyName("diff_7d")]
        public required DiffRate WeeklyDiff { get; init; }

        [JsonPropertyName("diff_30d")]
        public required DiffRate MonthlyDiff { get; init; }
    }
}
