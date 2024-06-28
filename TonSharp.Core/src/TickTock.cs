namespace TonSharp.Core
{
    public readonly struct TickTock
    {
        public required bool Tick { get; init; }
        public required bool Tock { get; init; }

        public static TickTock Load(Slice slice)
        {
            return new()
            {
                Tick = slice.LoadBit(),
                Tock = slice.LoadBit()
            };
        }

        public void Store(CellBuilder builder)
        {
            builder.StoreBit(Tick).StoreBit(Tock);
        }
    }
}
