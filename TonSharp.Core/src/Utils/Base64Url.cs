using System.Text;

namespace TonSharp.Core
{
    public static class Base64Url
    {
        public static bool TryEncode(ReadOnlySpan<byte> input, Span<char> output, out int charsWritten)
        {
            int pad = (3 - input.Length % 3) % 3;
            if (!Convert.TryToBase64Chars(input, output, out charsWritten))
                return false;
            output.Replace('+', '-');
            output.Replace('/', '_');
            charsWritten -= pad;
            return true;
        }

        public static int GetDecodedBytesCount(ReadOnlySpan<char> input)
        {
            int bytes = Encoding.UTF8.GetByteCount(input);
            int pad = input[^1] == '=' ? input[^2] == '=' ? 2 : 1 : 0;
            return bytes / 4 * 3 - pad;
        }

        public static bool TryDecode(ReadOnlySpan<char> input, Span<byte> output, out int bytesWritten)
        {
            int pad = (4 - input.Length % 4) % 4;
            Span<char> str = stackalloc char[input.Length + pad];
            input.CopyTo(str);
            str.Replace('-', '+');
            str.Replace('_', '/');
            if (pad > 0)
            {
                str[input.Length] = '=';
                if (pad == 2)
                    str[input.Length + 1] = '=';
            }
            return Convert.TryFromBase64Chars(str, output, out bytesWritten);
        }
    }
}
