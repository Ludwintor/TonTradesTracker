using System.Numerics;

namespace TonSharp.Core
{
    public static class DictKeys
    {
        public static UIntKey<T> UInt<T>(int bits) where T : struct, IBinaryInteger<T> 
            => new(bits);
    }
}
