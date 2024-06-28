using System.Security.Cryptography;

namespace TonSharp.Core
{
    // TODO: !!! SUPPORT EXOTIC CELLS
    public sealed class Cell
    {
        private readonly CellType _type;
        private readonly Bits _bits;
        private readonly Cell[] _refs;
        private readonly ushort _depth;
        private readonly byte _level;
        private readonly Buffer256 _hash;

        internal Cell(bool exotic = false, Bits? bits = null, Cell[]? refs = null)
        {
            _type = CellType.Ordinary;
            _bits = bits ?? new();
            _refs = refs ?? [];
            _depth = 0;
            _level = 0;

            // ordinary cell
            if (_bits.Length > CellBuilder.MAX_BITS)
                throw new Exception("Bits overflow");
            if (_refs.Length > CellBuilder.MAX_REFS)
                throw new Exception("Invalid number of references");

            // calculate depth, level and hash
            int fullBytes = (_bits.Length + 7) / 8;
            Span<byte> repr = stackalloc byte[fullBytes + 2 + _refs.Length * (2 + 32)];
            if (fullBytes > 0)
            {
                BitWriter writer = new(fullBytes * 8);
                writer.WriteBitsPadded(_bits);
                writer.Build().TryWriteBytes(repr.Slice(2, fullBytes), 0);
            }

            if (_refs.Length > 0)
            {
                Span<byte> depthArea = repr.Slice(fullBytes + 2, _refs.Length * 2);
                Span<byte> hashArea = repr[(fullBytes + 2 + _refs.Length * 2)..];
                for (int i = 0; i < _refs.Length; i++)
                {
                    Cell cell = _refs[i];
                    ushort depth = cell._depth;
                    ReadOnlySpan<byte> hash = cell._hash;
                    if (depth > _depth)
                        _depth = depth;
                    _level |= cell._level;

                    depthArea[i * 2 + 0] = (byte)(depth >> 8);
                    depthArea[i * 2 + 1] = (byte)(depth >> 0);
                    hash.CopyTo(hashArea.Slice(i * 32, 32));
                }
                _depth++;
            }

            (repr[0], repr[1]) = GetDescriptors();
            SHA256.HashData(repr, _hash);
        }

        public CellType Type => _type;

        public Bits Bits => _bits;

        public Cell[] Refs => _refs;

        public Buffer256 Hash => _hash;

        public static CellBuilder Begin() => new();

        public static Cell FromHex(ReadOnlySpan<char> hexBoc)
        {
            Span<char> lowerHex = stackalloc char[hexBoc.Length];
            hexBoc.ToLower(lowerHex, null);
            return BOC.Deserialize(Convert.FromHexString(lowerHex))[0];
        }

        public static Cell FromBase64(string base64Boc)
        {
            byte[] boc = new byte[Base64Url.GetDecodedBytesCount(base64Boc)];
            Base64Url.TryDecode(base64Boc, boc, out _);
            return BOC.Deserialize(boc)[0];
        }

        public static Cell FromBoc(byte[] boc)
            => BOC.Deserialize(boc)[0];

        public Slice BeginParse()
        {
            return new(_bits, _refs);
        }

        public Bits ToBoc(bool hasIndex = false, bool hasCrc32 = false)
        {
            return BOC.Serialize(this, hasIndex, hasCrc32);
        }

        internal (byte, byte) GetDescriptors()
        {
            return (
                (byte)(_refs.Length | (_type != CellType.Ordinary ? 8 : 0) | _level << 5), // refs descriptor
                (byte)((_bits.Length + 7) / 8 + _bits.Length / 8) // bits descriptor
                );
        }
    }
}
