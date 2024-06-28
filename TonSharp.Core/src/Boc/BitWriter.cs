using System.Numerics;

namespace TonSharp.Core
{
    // TON IS BIG ENDIAN
    public sealed class BitWriter
    {
        private readonly byte[] _buffer;
        private readonly int _length;
        private int _position;

        public BitWriter(int length = 1023)
        {
            _buffer = new byte[length / 8 + (length % 8 > 0 ? 1 : 0)];
            _length = length;
            _position = 0;
        }

        public int Length => _length;

        public int Position => _position;

        public void WriteBit(bool value)
        {
            if (_position == _length)
                throw new Exception("BitWriter overflow");
            if (value)
                _buffer[_position / 8] |= (byte)(1 << (7 - (_position % 8)));
            _position++;
        }

        public void WriteBits(Bits bits)
        {
            for (int i = 0; i < bits.Length; i++)
                WriteBit(bits[i]);
        }

        public void WriteBitsPadded(Bits bits)
        {
            WriteBits(bits);
            int padding = (bits.Length + 7) / 8 * 8 - bits.Length;
            if (padding == 0)
                return;
            if (_position + padding > _length)
                throw new Exception("BitWriter overflow");
            WriteBit(true);
            _position += padding - 1;
        }

        public void WriteByte(byte value)
        {
            WriteNumber(value, 8);
        }

        public void WriteBytes(ReadOnlySpan<byte> bytes)
        {
            if (bytes.IsEmpty)
                return; // throw? do we allow to "write" 0 bytes?
            // if we are currently aligned - it is much faster to just copy
            if (_position % 8 == 0)
            {
                if (_position + bytes.Length * 8 > _length)
                    throw new Exception("BitWriter overflow");
                bytes.CopyTo(_buffer.AsSpan()[(_position / 8)..]);
                _position += bytes.Length * 8;
            }
            else
            {
                for (int i = 0; i < bytes.Length; i++)
                    WriteNumber(bytes[i], 8);
            }
        }

        public void WriteUInt<T>(T value, int bits) where T : struct, IBinaryInteger<T>
        {
            int vBits = value.GetShortestBitLength();
            if (value < T.Zero || vBits > bits)
                throw new Exception($"Value overflow for {bits}. Got {value}");
            // because shift operators "rotates" when shifting on more bits than size of type itself
            // skip excess bits beforehand
            int excessBits = bits - vBits;
            if (excessBits > 0)
            {
                _position += excessBits;
                bits -= excessBits;
            }
            WriteNumber(value, bits);
        }

        public void WriteInt<T>(T value, int bits) where T : struct, IBinaryInteger<T>, ISignedNumber<T>
        {
            if (bits == 1)
            {
                if (value != T.Zero && value != T.NegativeOne)
                    throw new Exception($"Value must be 0 or -1 for {bits} bits. Got {value}");
                WriteBit(value == T.NegativeOne);
                return;
            }

            bool isNegative = value < T.Zero;
            // GetShortestBitLength return 31 bits for max value of signed int32 (and etc)
            // so to avoid overflow - we subtract one bit when integer is positive
            int vBits = value.GetShortestBitLength() - (isNegative ? 0 : 1);
            if (vBits > bits)
                throw new Exception($"Value overflow for {bits}. Got {value}");
            WriteBit(isNegative);
            int excessBits = bits - vBits;
            // write excess bits beforehand
            if (excessBits > 0)
            {
                if (isNegative)
                    WriteNumber(-1, excessBits);
                else
                    _position += excessBits;
                bits -= excessBits;
            }
            WriteNumber(value, bits - 1);
        }

        public void WriteVarUInt<T>(T value, int headerBits) where T : struct, IBinaryInteger<T>
        {
            if (value < T.Zero)
                throw new Exception($"Value must be non-negative. Got {value}");
            int bytes = WriteVarSize(value, headerBits);
            if (bytes > 0)
                WriteUInt(value, bytes * 8);
        }

        public void WriteVarInt<T>(T value, int headerBits) where T : struct, IBinaryInteger<T>, ISignedNumber<T>
        {
            int bytes = WriteVarSize(value, headerBits);
            if (bytes > 0)
                WriteInt(value, bytes * 8);
        }

        public void WriteCoins(Coins value)
        {
            WriteVarUInt(value.Value, 4);
        }

        public void WriteAddress(Address? address)
        {
            if (address == null)
            {
                WriteNumber(0, 2); // empty address
                return;
            }

            WriteNumber(2, 2 + 1); // internal address (2) + no anycast (0)
            WriteInt(address.Value.Workchain, 8);
            Buffer256 hash = address.Value.Hash;
            WriteBytes(hash);
            // TODO: write external address (separate method)
        }

        public Bits Build()
        {
            return new(_buffer, 0, _position);
        }

        private int WriteVarSize<T>(T value, int headerBits) where T : struct, IBinaryInteger<T>
        {
            if (value == T.Zero)
            {
                WriteUInt(0, headerBits);
                return 0;
            }

            int bitLength = value.GetShortestBitLength();
            int sizeBytes = bitLength / 8 + (bitLength % 8 > 0 ? 1 : 0);
            WriteUInt(sizeBytes, headerBits);
            return sizeBytes;
        }

        private void WriteNumber<T>(T value, int bits) where T : struct, IBinaryInteger<T>
        {
            if (_position + bits > _length)
                throw new Exception("BitWriter overflow");
            int tillByte = 8 - (_position % 8);
            if (tillByte < 8)
            {
                if (bits <= tillByte)
                {
                    byte b = byte.CreateTruncating(value << (tillByte - bits));
                    _buffer[_position / 8] |= (byte)(~(-1 << tillByte) & b);
                    _position += bits;
                    return;
                }
                else
                {
                    byte b = byte.CreateTruncating(value >> (bits - tillByte));
                    _buffer[_position / 8] |= (byte)(~(-1 << tillByte) & b);
                    _position += tillByte;
                }
                bits -= tillByte;
            }
            // TODO: can we make use of SIMD with vectorization?
            while (bits > 0)
            {
                if (bits >= 8)
                {
                    _buffer[_position / 8] = byte.CreateTruncating(value >> (bits - 8));
                    _position += 8;
                    bits -= 8;
                }
                else
                {
                    _buffer[_position / 8] = byte.CreateTruncating(value << (8 - bits));
                    _position += bits;
                    bits = 0;
                }
            }
        }
    }
}
