using System.Text;

namespace TradesTracker.App
{
    public static class Utils
    {
        public static void EscapeMarkdown(StringBuilder sb)
        {
            sb.Replace(".", "\\.");
            sb.Replace("(", "\\(");
            sb.Replace(")", "\\)");
            sb.Replace("_", "\\_");
            sb.Replace("*", "\\*");
            sb.Replace("[", "\\[");
            sb.Replace("]", "\\]");
            sb.Replace("~", "\\~");
            sb.Replace("`", "\\`");
            sb.Replace(">", "\\>");
            sb.Replace("#", "\\#");
            sb.Replace("+", "\\+");
            sb.Replace("-", "\\-");
            sb.Replace("=", "\\=");
            sb.Replace("|", "\\|");
            sb.Replace("!", "\\!");
        }

        public static string EscapeMarkdown(string value)
        {
            StringBuilder sb = new(value);
            EscapeMarkdown(sb);
            return sb.ToString();
        }

        public static string ShortAddress(string address, bool escapeMarkdown = false)
        {
            StringBuilder sb = new(11 + (escapeMarkdown ? 3 : 0));
            ReadOnlySpan<char> addressSpan = address.AsSpan();
            sb.Append(addressSpan[..4]).Append("...").Append(addressSpan[^4..]);
            if (escapeMarkdown)
                EscapeMarkdown(sb);
            return sb.ToString();
        }
    }
}
