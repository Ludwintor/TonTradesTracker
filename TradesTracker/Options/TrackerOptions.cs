namespace TradesTracker.Options
{
    public sealed class TrackerOptions
    {
        public const string TRACKER = "Tracker";

        public required long ChannelId { get; init; }

        public required string TokenAddress { get; init; }

        public required string PoolAddress { get; init; }

        public required int TradesPerPass { get; init; }

        public required int PassDelay { get; init; }

        public required string ExplorerUrl { get; init; }
    }
}
