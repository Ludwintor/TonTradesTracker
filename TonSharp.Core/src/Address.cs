namespace TonSharp.Core
{
    public readonly struct Address : IEquatable<Address>
    {
        private const int HASH_BYTES = 32;
        private const int FRIENDLY_BYTES = 36;
        private const int FRIENDLY_LENGTH = 48;
        private const byte BOUNCE_FLAG = 0x11; 
        private const byte NONBOUNCE_FLAG = 0x51;
        private const byte TEST_FLAG = 0x80;

        private readonly int _workchain;
        private readonly Buffer256 _hash;

        public Address(int workchain, ReadOnlySpan<byte> hash)
        {
            _workchain = workchain;
            hash.CopyTo(_hash);
        }

        /// <summary>
        /// Zero (null) address in basechain (workchain = 0)
        /// </summary>
        /// <remarks>This is NOT absent of address</remarks>
        public static Address Zero { get; } = new(0, default);

        public int Workchain => _workchain;

        public Buffer256 Hash => _hash;

        public static Address FromContract(int workchain, StateInit stateInit)
        {
            Buffer256 hash = Cell.Begin().Store(stateInit).End().Hash;
            return new(workchain, hash);
        }

        public static Address ParseRaw(ReadOnlySpan<char> str)
        {
            int colon = str.IndexOf(':');
            if (colon < 0)
                throw new Exception("Invalid raw address format");
            if (!int.TryParse(str[..colon], out int workchain))
                throw new Exception("Invalid raw address format");
            ReadOnlySpan<char> hashHex = str[(colon + 1)..];
            if (hashHex.Length != HASH_BYTES * 2)
                throw new Exception("Invalid raw address format");
            // TODO: why there's no Span-based Convert.FromHexString
            return new(workchain, Convert.FromHexString(hashHex));
        }

        public static Address ParseFriendly(ReadOnlySpan<char> str)
        {
            return ParseFriendly(str, out _, out _);
        }

        public static Address ParseFriendly(ReadOnlySpan<char> str, out bool isBounceable, out bool isTestOnly)
        {
            if (str.Length != FRIENDLY_LENGTH)
                throw new Exception($"Invalid friendly address length: {str.Length}");
            Span<byte> bytes = stackalloc byte[FRIENDLY_BYTES];
            if (!Base64Url.TryDecode(str, bytes, out int bytesWritten) || bytesWritten != FRIENDLY_BYTES)
                throw new Exception("Invalid friendly address format");
            ReadOnlySpan<byte> address = bytes[..(FRIENDLY_BYTES - 2)];
            ReadOnlySpan<byte> crc = bytes[(FRIENDLY_BYTES - 2)..];
            ushort computedCrc = CRC16.Compute(address);
            if ((crc[1] | crc[0] << 8) != computedCrc)
                throw new Exception($"Invalid checksum: {str}");
            byte flags = address[0];
            isTestOnly = false;
            if ((flags & TEST_FLAG) == TEST_FLAG)
            {
                isTestOnly = true;
                flags ^= TEST_FLAG;
            }
            if (flags != BOUNCE_FLAG && flags != NONBOUNCE_FLAG)
                throw new Exception("Invalid address flags");
            isBounceable = flags == BOUNCE_FLAG;
            int workchain = (sbyte)address[1];
            return new(workchain, address[2..]);
        }

        public string ToRawString()
        {
            return $"{_workchain}:{Convert.ToHexString(_hash).ToLower()}";
        }

        public string ToString(bool bounceable = true, bool testOnly = false)
        {
            Span<byte> buffer = stackalloc byte[FRIENDLY_BYTES];
            byte flags = bounceable ? BOUNCE_FLAG : NONBOUNCE_FLAG;
            if (testOnly)
                flags |= TEST_FLAG;

            buffer[0] = flags;
            buffer[1] = (byte)_workchain;
            _hash[..].CopyTo(buffer[2..(FRIENDLY_BYTES - 2)]);
            ushort crc = CRC16.Compute(buffer[..(FRIENDLY_BYTES - 2)]);
            buffer[FRIENDLY_BYTES - 2] = (byte)(crc >> 8);
            buffer[FRIENDLY_BYTES - 1] = (byte)crc;
            Span<char> str = stackalloc char[FRIENDLY_LENGTH];
            Base64Url.TryEncode(buffer, str, out _);
            return str.ToString();
        }

        public override string ToString()
        {
            return ToString(true, false);
        }

        public bool Equals(Address other)
        {
            return _workchain == other._workchain && _hash == other._hash;
        }

        public override bool Equals(object? obj)
        {
            return obj is Address address && Equals(address);
        }

        public static bool operator ==(Address left, Address right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Address left, Address right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_workchain, _hash);
        }
    }
}
