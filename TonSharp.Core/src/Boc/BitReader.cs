using System.Numerics;

namespace TonSharp.Core
{
    // TODO: !!! ADD CHECK FOR MAX VALUE SIZE
    public sealed class BitReader
    {
        private readonly Bits _bits;
        private int _position;

        public BitReader(Bits bits, int position = 0)
        {
            _bits = bits;
            _position = position;
        }

        public int Position => _position;

        public int Remaining => _bits.Length - _position;

        public void Skip(int bits)
        {
            if (bits < 0)
                throw new ArgumentOutOfRangeException(nameof(bits), "Bits to skip must be non-negative");
            if (_position + bits > _bits.Length)
                throw new Exception("BitReader underflow");
            _position += bits;
        }

        public bool LoadBit()
        {
            bool result = _bits[_position];
            _position++;
            return result;
        }

        public bool PreloadBit()
        {
            return _bits[_position];
        }

        public Bits LoadBits(int bits)
        {
            Bits result = _bits.Slice(_position, bits);
            _position += bits;
            return result;
        }

        public Bits PreloadBits(int bits)
        {
            return _bits.Slice(_position, bits);
        }

        public Bits LoadBitsPadded(int bits)
        {
            Bits result = PreloadBitsPadded(bits);
            _position += bits;
            return result;
        }

        public Bits PreloadBitsPadded(int bits)
        {
            if (bits % 8 != 0)
                throw new Exception("Loading padded bits requires byte-aligned number of bits");
            int length = bits;
            while (!_bits[_position + length - 1])
                length--;
            return _bits.Slice(_position, length - 1);
        }

        public byte[] LoadBytes(int bytes)
        {
            byte[] result = new byte[bytes];
            LoadBytes(result);
            return result;
        }

        public void LoadBytes(Span<byte> bytes)
        {
            PreloadBytes(bytes, _position);
            _position += bytes.Length * 8;
        }

        public byte[] PreloadBytes(int bytes)
        {
            byte[] result = new byte[bytes];
            PreloadBytes(result);
            return result;
        }

        public void PreloadBytes(Span<byte> bytes)
        {
            PreloadBytes(bytes, _position);
        }

        public T LoadUInt<T>(int bits) where T : IBinaryInteger<T>
        {
            T result = PreloadUInt<T>(bits, _position);
            _position += bits;
            return result;
        }

        public T PreloadUInt<T>(int bits) where T : IBinaryInteger<T>
        {
            return PreloadUInt<T>(bits, _position);
        }

        public T LoadInt<T>(int bits) where T : IBinaryInteger<T>, ISignedNumber<T>
        {
            T result = PreloadInt<T>(bits, _position);
            _position += bits;
            return result;
        }

        public T PreloadInt<T>(int bits) where T : IBinaryInteger<T>, ISignedNumber<T>
        {
            return PreloadInt<T>(bits, _position);
        }

        public T LoadVarUInt<T>(int bits) where T : IBinaryInteger<T>
        {
            int bytesSize = LoadUInt<int>(bits);
            return LoadUInt<T>(bytesSize * 8);
        }

        public T PreloadVarUInt<T>(int bits) where T : IBinaryInteger<T>
        {
            int bytesSize = PreloadUInt<int>(bits, _position);
            return PreloadUInt<T>(bytesSize * 8, _position + bits);
        }

        public T LoadVarInt<T>(int bits) where T : IBinaryInteger<T>, ISignedNumber<T>
        {
            int bytesSize = LoadUInt<int>(bits);
            return LoadInt<T>(bytesSize * 8);
        }

        public T PreloadVarInt<T>(int bits) where T : IBinaryInteger<T>, ISignedNumber<T>
        {
            int bytesSize = PreloadUInt<int>(bits, _position);
            return PreloadInt<T>(bytesSize * 8, _position + bits);
        }

        public Coins LoadCoins()
        {
            return new(LoadVarUInt<UInt128>(4));
        }

        public Coins PreloadCoins()
        {
            return new(PreloadVarUInt<UInt128>(4));
        }

        // TODO: add reading external address
        public Address LoadAddress()
        {
            // 2 - type for internal address TODO: move this magic number to const
            int type = PreloadUInt<int>(2, _position);
            if (type == 2)
                return LoadAddressInternal();
            throw new Exception("Invalid address type loaded");
        }

        public Address? LoadMaybeAddress()
        {
            int type = PreloadUInt<int>(2, _position);
            if (type == 0)
            {
                _position += 2;
                return null;
            }
            if (type == 2)
                return LoadAddressInternal();
            throw new Exception("Invalid address type loaded");
        }

        // TODO: READ BY FULL BYTES. IT WILL BE MUCH FASTER
        private T PreloadUInt<T>(int bits, int position) where T : IBinaryInteger<T>
        {
            if (bits == 0)
                return T.Zero;

            T result = T.Zero;
            for (int i = 0; i < bits; i++)
                if (_bits[position + i])
                    result |= T.One << (bits - i - 1);
            return result;
        }

        private T PreloadInt<T>(int bits, int position) where T : IBinaryInteger<T>, ISignedNumber<T>
        {
            if (bits == 0)
                return T.Zero;

            T result = T.Zero;
            for (int i = 0; i < bits - 1; i++)
                if (_bits[position + 1 + i])
                    result |= T.One << (bits - i - 2);
            if (_bits[position])
                result -= T.One << (bits - 1);
            return result;
        }

        private void PreloadBytes(Span<byte> buffer, int position)
        {
            // if we are currently aligned - it is much faster to just copy
            if (_bits.TryWriteBytes(buffer, position))
                return;

            for (int i = 0; i < buffer.Length; i++)
                buffer[i] = PreloadUInt<byte>(8, position + i * 8);
        }

        private Address LoadAddressInternal()
        {
            // skip address type so _postion + 2
            if (_bits[_position + 2]) // no anycast supported
                throw new Exception("Invalid address");
            int workchain = PreloadInt<int>(8, _position + 3);
            Buffer256 hash = default;
            PreloadBytes(hash, _position + 11);
            _position += 267;
            return new(workchain, hash);
        }
    }
}
