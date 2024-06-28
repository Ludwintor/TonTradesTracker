using System.Diagnostics.CodeAnalysis;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace TonSharp.Core
{
    [InlineArray(32)]
    public struct Buffer256 : IEquatable<Buffer256>
    {
#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable IDE0044 // Add readonly modifier
        private byte _bytes;
#pragma warning restore IDE0044 // Add readonly modifier
#pragma warning restore IDE0051 // Remove unused private members

        public readonly bool Equals(Buffer256 other)
        {
            return this[..].SequenceEqual(other);
        }

        public override readonly bool Equals([NotNullWhen(true)] object? obj)
        {
            return obj is Buffer256 other && Equals(other);
        }

        public override readonly int GetHashCode()
        {
            int result;
            if (Vector128.IsHardwareAccelerated)
            {
                Vector128<int> lower128;
                Vector128<int> upper128;
                if (Vector256.IsHardwareAccelerated)
                {
                    Vector256<int> vec = Vector256.Create<byte>(this).AsInt32();
                    lower128 = Vector256.GetLower(vec);
                    upper128 = Vector256.GetUpper(vec);
                }
                else
                {
                    lower128 = Vector128.Create(this[0..16]).AsInt32();
                    upper128 = Vector128.Create(this[16..]).AsInt32();
                }
                Vector128<int> xor128 = Vector128.Xor(lower128, upper128);
                Vector64<int> lower64 = Vector128.GetLower(xor128);
                Vector64<int> upper64 = Vector128.GetUpper(xor128);
                Vector64<int> xor64 = Vector64.Xor(lower64, upper64);
                return xor64[0] ^ xor64[1];
            }
            result = 0;
            for (int i = 0; i < 8; i++)
            {
                int idx = i * 4;
                int xor = this[idx + 0] << ((idx + 0) * 8);
                xor |= this[idx + 1] << ((idx + 1) * 8);
                xor |= this[idx + 2] << ((idx + 2) * 8);
                xor |= this[idx + 3] << ((idx + 3) * 8);
                result ^= xor;
            }
            return result;
        }

        public static bool operator ==(Buffer256 left, Buffer256 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Buffer256 left, Buffer256 right)
        {
            return !(left == right);
        }
    }
}
