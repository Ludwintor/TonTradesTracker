namespace TonSharp.Core
{
    public interface ICellStorable<T>
    {
        static abstract T Load(Slice slice);

        void Store(CellBuilder builder);
    }
}
