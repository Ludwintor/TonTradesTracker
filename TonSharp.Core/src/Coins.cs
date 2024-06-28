using System.Globalization;
using System.Numerics;

namespace TonSharp.Core
{
    // TODO: should to implement IBinaryInteger<Coins> and etc? they are as big as f
    // coins can only take up to 120 bits so we can back it with UInt128 to avoid using BigInteger
    public readonly struct Coins : IEquatable<Coins>, IMinMaxValue<Coins>
    {
        private const decimal NANO = 1000000000;

        public static Coins MaxValue { get; } = new((UInt128.One << 120) - 1);

        public static Coins MinValue => Zero;

        public static Coins One => new(UInt128.One);

        public static Coins Zero => new(UInt128.Zero);

        private readonly UInt128 _value;

        public Coins(UInt128 value)
        {
            _value = value & MaxValue._value;
        }

        internal UInt128 Value => _value;

        public static Coins ToDecimals(ReadOnlySpan<char> str, int decimals)
        {
            decimal dec = decimal.Parse(str, CultureInfo.InvariantCulture);
            return ToDecimals(dec, decimals);
        }

        public static Coins ToDecimals(decimal value, int decimals)
        {
            if (decimals < 0 || decimals > 18)
                throw new ArgumentOutOfRangeException(nameof(decimals), $"Decimals outside supported range. 0 <= {decimals} <= 18");
            if (value < 0)
                throw new Exception("Value must be non-negative number");
            return new((UInt128)(value * (decimal)Math.Pow(10, decimals)));
        }

        public static Coins ToNano(ReadOnlySpan<char> str)
        {
            decimal dec = decimal.Parse(str, CultureInfo.InvariantCulture);
            return ToNano(dec);
        }

        public static Coins ToNano(decimal value)
        {
            if (value < 0)
                throw new Exception("Value must be non-negative number");
            return new((UInt128)(value * NANO));
        }

        public static Coins operator &(Coins left, Coins right)
            => new(left._value & right._value);

        public static Coins operator |(Coins left, Coins right)
            => new(left._value | right._value);

        public static Coins operator ^(Coins left, Coins right)
            => new(left._value ^ right._value);

        public static Coins operator ~(Coins value)
            => new((~value._value) & MaxValue._value);

        public static bool operator >(Coins left, Coins right)
            => left._value > right._value;

        public static bool operator >=(Coins left, Coins right)
            => left._value >= right._value;

        public static bool operator <(Coins left, Coins right)
            => left._value < right._value;

        public static bool operator <=(Coins left, Coins right)
            => left._value <= right._value;

        public static Coins operator %(Coins left, Coins right)
            => new(left._value % right._value);

        public static Coins operator --(Coins value)
            => new(unchecked(value._value - 1) & MaxValue._value);

        public static Coins operator /(Coins left, Coins right)
            => new(left._value / right._value);

        public static bool operator ==(Coins left, Coins right)
            => left._value == right._value;

        public static bool operator !=(Coins left, Coins right)
            => left._value != right._value;

        public static Coins operator ++(Coins value)
            => new(unchecked(value._value + 1) & MaxValue._value);

        public static Coins operator *(Coins left, Coins right)
            => new(unchecked(left._value * right._value) & MaxValue._value);

        public static Coins operator -(Coins left, Coins right)
            => new(unchecked(left._value - right._value) & MaxValue._value);

        public static Coins operator -(Coins value)
            => new(-value._value & MaxValue._value);

        public static Coins operator +(Coins left, Coins right)
            => new(unchecked(left._value + right._value) & MaxValue._value);

        public static Coins operator +(Coins value)
            => new(+value._value & MaxValue._value);

        public static Coins operator <<(Coins value, int shiftAmount)
            => new(value._value << (shiftAmount % 120) & MaxValue._value);

        public static Coins operator >>(Coins value, int shiftAmount)
            => new((value._value & MaxValue._value) >> (shiftAmount % 120));

        public static Coins operator >>>(Coins value, int shiftAmount)
            => new((value._value & MaxValue._value) >>> (shiftAmount % 120));

        public static implicit operator Coins(UInt128 value)
            => new(value);

        public static implicit operator Coins(ulong value)
            => new(value);

        public static implicit operator Coins(uint value)
            => new(value);

        public decimal FromDecimals(int decimals)
        {
            if (decimals < 0 || decimals > 18)
                throw new ArgumentOutOfRangeException(nameof(decimals), $"Decimals outside supported range. 0 <= {decimals} <= 18");
            return (decimal)_value / (decimal)Math.Pow(10, decimals);
        }

        public decimal FromNano()
        {
            return (decimal)_value / NANO;
        }

        public override bool Equals(object? obj)
        {
            return obj is Coins coins && Equals(coins);
        }

        public bool Equals(Coins other)
        {
            return _value.Equals(other._value);
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }
    }
}
