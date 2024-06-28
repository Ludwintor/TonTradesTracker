namespace TonSharp.Core
{
    // crc16 ccitt with poly = 0x1021 and init = 0
    public static class CRC16
    {
        private const ushort POLY = 0x1021;

        private static readonly ushort[] _table;

        static CRC16()
        {
            _table = new ushort[256];
            for (int i = 0; i < 256; i++)
            {
                ushort value = 0;
                ushort a = (ushort)(i << 8);
                for (int j = 0; j < 8; j++)
                {
                    if (((value ^ a) & 0x8000) != 0)
                        value = (ushort)((value << 1) ^ POLY);
                    else
                        value <<= 1;
                    a <<= 1;
                }
                _table[i] = value;
            }
        }

        public static ushort Compute(ReadOnlySpan<byte> bytes)
        {
            ushort crc = 0;
            for (int i = 0; i < bytes.Length; i++)
                crc = (ushort)((crc << 8) ^ _table[(crc >> 8) ^ bytes[i]]);
            return crc;
        }
    }
}
