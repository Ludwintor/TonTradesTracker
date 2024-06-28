using System.Text;

namespace TradesTracker.Dedust
{
    public sealed class UriQueryBuilder
    {
        public string BaseUrl { get; }

        private readonly StringBuilder _sb;

        public UriQueryBuilder(string url)
        {
            BaseUrl = url;
            _sb = new(url);
        }

        public UriQueryBuilder AddParameter(string name, string? value)
        {
            if (value == null)
                return this;

            _sb.Append(_sb.Length == BaseUrl.Length ? '?' : '&')
               .Append(Uri.EscapeDataString(name)).Append('=').Append(Uri.EscapeDataString(value));

            return this;
        }

        public string Build()
        {
            return _sb.ToString();
        }
    }
}
