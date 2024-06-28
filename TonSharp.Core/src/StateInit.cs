namespace TonSharp.Core
{
    // TODO: Implement dictionary
    public readonly struct StateInit : ICellStorable<StateInit>
    {
        public byte? SplitDepth { get; init; }
        public TickTock? Special { get; init; }
        public Cell? Code { get; init; }
        public Cell? Data { get; init; }

        public static StateInit Load(Slice slice)
        {
            StateInit result = new()
            {
                SplitDepth = slice.LoadBit() ? slice.LoadUInt<byte>(5) : null,
                Special = slice.LoadBit() ? TickTock.Load(slice) : null,
                Code = slice.LoadMaybeRef(),
                Data = slice.LoadMaybeRef()
            };
            // FOR NOW just skip library
            slice.LoadMaybeRef();
            return result;
        }

        public void Store(CellBuilder builder)
        {
            builder.StoreMaybeUInt(SplitDepth, 5);
            if (Special != null)
            {
                builder.StoreBit(true);
                Special.Value.Store(builder);
            }
            else
            {
                builder.StoreBit(false);
            }
            builder.StoreMaybeRef(Code);
            builder.StoreMaybeRef(Data);
            builder.StoreMaybeRef(null); // FOR NOW just store 0 maybe bit
        }
    }
}
