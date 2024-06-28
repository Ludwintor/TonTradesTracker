using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace TonSharp.Core
{
    public sealed class CellBuilder
    {
        public const int MAX_BITS = 1023;
        public const int MAX_BYTES = MAX_BITS / 8;
        public const int MAX_REFS = 4;

        private readonly BitWriter _bits;
        private List<Cell>? _refs;

        public CellBuilder()
        {
            _bits = new(MAX_BITS);
        }

        public int Bits => _bits.Position;

        public int Refs => _refs?.Count ?? 0;

        public int AvailableBits => _bits.Length - _bits.Position;

        public int AvailableRefs => MAX_REFS - Refs;

        public CellBuilder StoreBit(bool value)
        {
            _bits.WriteBit(value);
            return this;
        }

        public CellBuilder StoreBits(Bits bits)
        {
            _bits.WriteBits(bits);
            return this;
        }

        public CellBuilder StoreByte(byte value)
        {
            _bits.WriteByte(value);
            return this;
        }

        public CellBuilder StoreBytes(ReadOnlySpan<byte> bytes)
        {
            _bits.WriteBytes(bytes);
            return this;
        }

        public CellBuilder StoreMaybeBytes(byte[]? bytes)
        {
            return StoreMaybeBytes(bytes ?? ReadOnlySpan<byte>.Empty);
        }

        // if bytes span is empty - then it is "null"
        public CellBuilder StoreMaybeBytes(ReadOnlySpan<byte> bytes)
        {
            if (!bytes.IsEmpty)
            {
                _bits.WriteBit(true);
                _bits.WriteBytes(bytes);
            }
            else
            {
                _bits.WriteBit(false);
            }
            return this;
        }

        public CellBuilder StoreUInt<T>(T value, int bits) where T : struct, IBinaryInteger<T>
        {
            _bits.WriteUInt(value, bits);
            return this;
        }

        public CellBuilder StoreMaybeUInt<T>(T? value, int bits) where T : struct, IBinaryInteger<T>
        {
            if (StoreMaybeBit(ref value))
                _bits.WriteUInt(value.Value, bits);
            return this;
        }

        public CellBuilder StoreInt<T>(T value, int bits) where T : struct, IBinaryInteger<T>, ISignedNumber<T>
        {
            _bits.WriteInt(value, bits);
            return this;
        }

        public CellBuilder StoreMaybeInt<T>(T? value, int bits) where T : struct, IBinaryInteger<T>, ISignedNumber<T>
        {
            if (StoreMaybeBit(ref value))
                _bits.WriteInt(value.Value, bits);
            return this;
        }

        public CellBuilder StoreVarUInt<T>(T value, int bits) where T : struct, IBinaryInteger<T>
        {
            _bits.WriteVarUInt(value, bits);
            return this;
        }

        public CellBuilder StoreMaybeVarUInt<T>(T? value, int bits) where T : struct, IBinaryInteger<T>
        {
            if (StoreMaybeBit(ref value))
                _bits.WriteVarUInt(value.Value, bits);
            return this;
        }

        public CellBuilder StoreVarInt<T>(T value, int bits) where T : struct, IBinaryInteger<T>, ISignedNumber<T>
        {
            _bits.WriteVarInt(value, bits);
            return this;
        }

        public CellBuilder StoreMaybeVarInt<T>(T? value, int bits) where T : struct, IBinaryInteger<T>, ISignedNumber<T>
        {
            if (StoreMaybeBit(ref value))
                _bits.WriteVarInt(value.Value, bits);
            return this;
        }

        public CellBuilder StoreCoins(Coins value)
        {
            _bits.WriteCoins(value);
            return this;
        }

        public CellBuilder StoreMaybeCoins(Coins? value)
        {
            if (StoreMaybeBit(ref value))
                _bits.WriteCoins(value.Value);
            return this;
        }

        public CellBuilder StoreAddress(Address? value)
        {
            _bits.WriteAddress(value);
            return this;
        }

        public CellBuilder StoreRef(Cell cell)
        {
            if (Refs >= MAX_REFS)
                throw new Exception("Too many references");
            _refs ??= new List<Cell>(MAX_REFS);
            _refs.Add(cell);
            return this;
        }

        public CellBuilder StoreMaybeRef(Cell? cell)
        {
            if (StoreMaybeBit(ref cell))
                StoreRef(cell);
            return this;
        }

        public CellBuilder StoreSlice()
        {
            throw new NotImplementedException();
        }

        public CellBuilder StoreMaybeSlice()
        {
            throw new NotImplementedException();
        }

        public CellBuilder StoreStringTail(ReadOnlySpan<char> value, Encoding? encoding = null)
        {
            WriteStringTail(value, encoding ?? Encoding.UTF8);
            return this;
        }

        public CellBuilder StoreMaybeStringTail(string? value, Encoding? encoding = null)
        {
            return StoreMaybeStringTail(value ?? ReadOnlySpan<char>.Empty, encoding);
        }

        public CellBuilder StoreMaybeStringTail(ReadOnlySpan<char> value, Encoding? encoding = null)
        {
            if (!value.IsEmpty)
            {
                _bits.WriteBit(true);
                WriteStringTail(value, encoding ?? Encoding.UTF8);
            }
            else
            {
                _bits.WriteBit(false);
            }
            return this;
        }

        public CellBuilder StoreStringRefTail(ReadOnlySpan<char> value, Encoding? encoding = null)
        {
            StoreRef(new CellBuilder().StoreStringTail(value, encoding).End());
            return this;
        }

        public CellBuilder StoreMaybeStringRefTail(string? value, Encoding? encoding = null)
        {
            return StoreMaybeStringRefTail(value ?? ReadOnlySpan<char>.Empty, encoding);
        }

        public CellBuilder StoreMaybeStringRefTail(ReadOnlySpan<char> value, Encoding? encoding = null)
        {
            if (!value.IsEmpty)
            {
                _bits.WriteBit(true);
                StoreStringRefTail(value, encoding ?? Encoding.UTF8);
            }
            else
            {
                _bits.WriteBit(false);
            }
            return this;
        }

        // TODO: StoreDict

        public CellBuilder Store<T>(T value) where T : ICellStorable<T>
        {
            value.Store(this);
            return this;
        }

        public Cell End(bool isExotic = false)
        {
            return new(isExotic, _bits.Build(), _refs?.ToArray());
        }

        private bool StoreMaybeBit<T>([NotNullWhen(true)] ref readonly T? value)
        {
            bool notNull = value != null;
            _bits.WriteBit(notNull);
            return notNull;
        }

        private void WriteStringTail(ReadOnlySpan<char> value, Encoding encoding)
        {
            if (value.IsEmpty)
                return;
            Span<byte> bytes = stackalloc byte[encoding.GetByteCount(value)];
            encoding.GetBytes(value, bytes);
            int freeBytes = AvailableBits / 8;
            if (bytes.Length <= freeBytes)
            {
                _bits.WriteBytes(bytes);
                return;
            }
            // write to this cell from start if there's space available
            if (freeBytes > 0)
            {
                _bits.WriteBytes(bytes[..freeBytes]);
                bytes = bytes[freeBytes..];
            }
            Cell? current = null;
            // if last cell will have some space left - write these last bytes now
            int tailedBytes = bytes.Length % MAX_BYTES;
            if (tailedBytes > 0)
            {
                CellBuilder b = new();
                b._bits.WriteBytes(bytes[^tailedBytes..]);
                bytes = bytes[..^tailedBytes];
                current = b.End();
            }
            
            // now we can divide what's left on 127 byte chunks
            // write bytes from end to start
            while (!bytes.IsEmpty)
            {
                CellBuilder next = new();
                next._bits.WriteBytes(bytes[^MAX_BYTES..]);
                bytes = bytes[..^MAX_BYTES];
                if (current != null)
                    next.StoreRef(current);
                current = next.End();
            }
            Debug.Assert(current != null);
            StoreRef(current);
        }
    }
}
