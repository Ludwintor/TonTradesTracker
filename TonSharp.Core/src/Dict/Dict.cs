namespace TonSharp.Core
{
    public interface IDictKey<T> where T : notnull, IEquatable<T>
    {
        int Bits { get; }

        void Serialize(BitWriter writer, T key);

        T Deserialize(BitReader reader);
    }

    public interface IDictValue<T>
    {
        int Bits { get; }

        void Serialize(T key, CellBuilder builder);

        T Deserialize(Slice slice);
    }

    public sealed class Dict<TKey, TValue> where TKey : notnull, IEquatable<TKey>
    {
        private readonly Dictionary<TKey, TValue> _dict;
        private readonly IDictKey<TKey> _key;
        private readonly IDictValue<TValue> _value;

        public Dict(IDictKey<TKey> key, IDictValue<TValue> value)
        {
            _dict = [];
            _key = key;
            _value = value;
        }

        public static Dict<TKey, TValue> Load(Slice slice, TKey key, TValue value)
        {
            throw new NotImplementedException();
        }

        public void Store(CellBuilder builder)
        {
            if (_dict.Count == 0)
            {
                builder.StoreBit(false);
                return;
            }
            builder.StoreBit(true);
            CellBuilder dc = Cell.Begin();
            Serialize(dc);
            builder.StoreRef(dc.End());
        }

        private void Serialize(CellBuilder builder)
        {
            KeyValuePair<TKey, TValue>[] nodes = [.._dict];
            WriteEdge(nodes, builder);
        }

        private void WriteEdge(KeyValuePair<TKey, TValue>[] nodes, CellBuilder builder)
        {
            
        }

        private void WriteLabel()
        {

        }
    }
}
