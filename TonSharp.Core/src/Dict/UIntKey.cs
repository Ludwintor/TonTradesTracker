using System.Numerics;

namespace TonSharp.Core
{
    public class UIntKey<T> : IDictKey<T> where T : struct, IBinaryInteger<T>
    {
        public UIntKey(int bits)
        {
            Bits = bits;
        }

        public int Bits { get; }

        public void Serialize(BitWriter writer, T key)
        {
            writer.WriteUInt(key, Bits);
        }

        public T Deserialize(BitReader reader)
        {
            return reader.LoadUInt<T>(Bits);
        }
    }
}
