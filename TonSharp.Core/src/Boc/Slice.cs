using System.Numerics;

namespace TonSharp.Core
{
    public sealed class Slice
    {
        private readonly BitReader _reader;
        private readonly Cell[] _refs;
        private int _refsPosition;

        internal Slice(Bits bits, Cell[] refs)
        {
            _reader = new(bits);
            _refs = refs;
            _refsPosition = 0;
        }

        public int RemainingBits => _reader.Remaining;

        public int BitsPosition => _reader.Position;

        public int RemainingRefs => _refs.Length - _refsPosition;

        public int RefsPosition => _refsPosition;

        public Slice Skip(int bits)
        {
            _reader.Skip(bits);
            return this;
        }

        public bool LoadBit()
            => _reader.LoadBit();

        public bool PreloadBit()
            => _reader.PreloadBit();

        public byte[] LoadBytes(int bytes)
            => _reader.LoadBytes(bytes);

        public void LoadBytes(Span<byte> bytes)
            => _reader.LoadBytes(bytes);

        public byte[] PreloadBytes(int bytes)
            => _reader.PreloadBytes(bytes);

        public void PreloadBytes(Span<byte> bytes)
            => _reader.PreloadBytes(bytes);

        public Bits LoadBits(int bits)
            => _reader.LoadBits(bits);

        public Bits PreloadBits(int bits)
            => _reader.PreloadBits(bits);

        public T LoadUInt<T>(int bits) where T : struct, IBinaryInteger<T>
            => _reader.LoadUInt<T>(bits);

        public T PreloadUInt<T>(int bits) where T : struct, IBinaryInteger<T>
            => _reader.PreloadUInt<T>(bits);

        public T LoadInt<T>(int bits) where T : struct, IBinaryInteger<T>, ISignedNumber<T>
            => _reader.LoadInt<T>(bits);

        public T PreloadInt<T>(int bits) where T : struct, IBinaryInteger<T>, ISignedNumber<T>
            => _reader.PreloadInt<T>(bits);

        public T LoadVarUInt<T>(int bits) where T : struct, IBinaryInteger<T>
            => _reader.LoadVarUInt<T>(bits);

        public T PreloadVarUInt<T>(int bits) where T : struct, IBinaryInteger<T>
            => _reader.PreloadVarUInt<T>(bits);

        public T LoadVarInt<T>(int bits) where T : struct, IBinaryInteger<T>, ISignedNumber<T>
            => _reader.LoadVarInt<T>(bits);

        public T PreloadVarInt<T>(int bits) where T : struct, IBinaryInteger<T>, ISignedNumber<T>
            => _reader.PreloadVarInt<T>(bits);

        public Coins LoadCoins()
            => _reader.LoadCoins();

        public Coins PreloadCoins()
            => _reader.PreloadCoins();

        public Address LoadAddress()
            => _reader.LoadAddress();

        public Address? LoadMaybeAddress()
            => _reader.LoadMaybeAddress();

        public Cell LoadRef()
        {
            if (_refsPosition >= _refs.Length)
                throw new Exception("No more refs");
            return _refs[_refsPosition++];
        }

        public Cell PreloadRef()
        {
            if (_refsPosition >= _refs.Length)
                throw new Exception("No more refs");
            return _refs[_refsPosition];
        }

        public Cell? LoadMaybeRef()
            => _reader.LoadBit() ? LoadRef() : null;

        public Cell? PreloadMaybeRef()
            => _reader.PreloadBit() ? PreloadRef() : null;

        // TODO: Implement load string and dict
        public string LoadStringSnake()
            => throw new NotImplementedException();

        public string PreloadStringSnake()
            => throw new NotImplementedException();

        public T Load<T>() where T : ICellStorable<T>
            => T.Load(this);
    }
}
