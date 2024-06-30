using System.Numerics;

namespace TonSharp.Core
{
    public readonly struct Bits : IEquatable<Bits>
    {
        public static Bits Empty => new([], 0, 0);

        private readonly byte[] _buffer;
        private readonly int _offset;
        private readonly int _length;

        public Bits(byte[] buffer, int offset, int length)
        {
            _buffer = buffer;
            _offset = offset;
            _length = length;
        }

        public int Offset => _offset;

        public int Length => _length;

        public bool this[int index]
        {
            get
            {
                if (index < 0 || index >= _length)
                    throw new IndexOutOfRangeException($"Index is out of bounds. 0 <= {index} < {_length}");
                int byteIndex = (_offset + index) / 8;
                int bitIndex = 7 - (_offset + index) % 8; // ton is big endian
                return (_buffer[byteIndex] & (1 << bitIndex)) != 0;
            }
        }

        public Bits this[Range range]
        {
            get
            {
                int start = range.Start.IsFromEnd ? _length - range.Start.Value : range.Start.Value;
                int end = range.End.IsFromEnd ? _length - range.End.Value : range.End.Value;
                if (start > end)
                    throw new IndexOutOfRangeException($"Start must be less or equal to end. {start} <= {end}");
                return Slice(start, end - start);
            }
        }

        public Bits Slice(int offset, int length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), $"Length must be non-negative number. Got {length}");
            if (offset < 0 || offset >= _length)
                throw new ArgumentOutOfRangeException(nameof(offset), $"Offset out of bounds. 0 <= {offset} < {_length}");
            if (length == 0)
                return Empty;

            if (offset + length > _length)
                throw new ArgumentOutOfRangeException(null, $"Out of bounds. Offset {offset} + Length {length} > {_length}");

            return new(_buffer, _offset + offset, length);
        }

        public bool TryWriteBytes(Span<byte> destination, int offset)
        {
            if (offset < 0 || offset >= _length)
                throw new ArgumentOutOfRangeException(nameof(offset), $"Offset out of bounds. 0 <= {offset} < {_length}");
            if (offset + destination.Length > _length)
                throw new ArgumentOutOfRangeException(null, $"Out of bounds. Offset {offset} + Length {destination.Length} > {_length}");

            if ((_offset + offset) % 8 != 0)
                return false;

            int start = (_offset + offset) / 8;
            int end = start + destination.Length;
            _buffer.AsSpan()[start..end].CopyTo(destination);
            return true;
        }

        public string ToHexString()
        {
            return Convert.ToHexString(_buffer).ToLower();
        }

        public string ToBase64String()
        {
            return Convert.ToBase64String(_buffer);
        }

        public override string ToString()
        {
            return ToHexString();
        }

        public bool Equals(Bits other)
        {
            if (_length != other._length)
                return false;
            for (int i = 0; i < _length; i++)
                if (this[i] != other[i])
                    return false;

            return true;
        }

        public override bool Equals(object? obj)
        {
            return obj is Bits bits && Equals(bits);
        }

        public static bool operator ==(Bits left, Bits right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Bits left, Bits right)
        {
            return !(left == right);
        }

        // TODO: optimize
        public override int GetHashCode()
        {
            uint result = 0;
            for (int i = 0; i < _length; i++)
                result ^= BitOperations.RotateLeft(this[i] ? 1u : 0u, i);
            return (int)result;
        }
    }
}
