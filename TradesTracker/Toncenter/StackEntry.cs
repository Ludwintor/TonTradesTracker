using System.Text.Json.Serialization;

namespace TradesTracker.Toncenter
{
    public sealed class StackEntry
    {
        [JsonPropertyName("type")]
        public required string Type { get; init; }

        [JsonPropertyName("value")]
        public required string Value { get; init; }
    }
}
