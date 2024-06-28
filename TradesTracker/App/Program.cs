using Telegram.Bot;
using TradesTracker.Dedust;
using TradesTracker.Options;
using TradesTracker.Tonapi;
using TradesTracker.Toncenter;

namespace TradesTracker.App
{
    public class Program
    {
        private const string BOT_TOKEN_KEY = "BOT_TOKEN";
        private const string TONCENTER_TOKEN_KEY = "TONCENTER_TOKEN";
        private const string DEDUST_URL_KEY = "DedustUrl";
        private const string TONCENTER_URL_KEY = "ToncenterUrl";
        private const string TONAPI_URL_KEY = "TonapiUrl";
        private const string CONFIG_FILE = "config.json";

        public static void Main(string[] args)
        {
            var b = Host.CreateApplicationBuilder(args);
            if (!b.Environment.IsDevelopment())
                b.Configuration.AddJsonFile(CONFIG_FILE);

            b.Services.AddHostedService<TradesService>();
            b.Services.Configure<TrackerOptions>(b.Configuration.GetSection(TrackerOptions.TRACKER));

            b.Services.AddHttpClient<IDedustClient, DedustClient>(c =>
            {
                string? url = b.Configuration[DEDUST_URL_KEY];
                ArgumentException.ThrowIfNullOrEmpty(url, nameof(url));

                c.BaseAddress = new(url);
            }).RemoveAllLoggers();
            b.Services.AddHttpClient<IToncenterClient, ToncenterClient>(c =>
            {
                string? url = b.Configuration[TONCENTER_URL_KEY];
                string? toncenterToken = b.Configuration[TONCENTER_TOKEN_KEY];
                ArgumentException.ThrowIfNullOrEmpty(url, nameof(url));

                c.BaseAddress = new(url);
                if (!string.IsNullOrEmpty(toncenterToken))
                    c.DefaultRequestHeaders.Add("X-API-Key", toncenterToken);
            }).RemoveAllLoggers();
            b.Services.AddHttpClient<ITonapiClient, TonapiClient>(c =>
            {
                string? url = b.Configuration[TONAPI_URL_KEY];
                ArgumentException.ThrowIfNullOrEmpty(url, nameof(url));

                c.BaseAddress = new(url);
            }).RemoveAllLoggers();

            b.Services.AddHttpClient<ITelegramBotClient, TelegramBotClient>(c =>
            {
                string? botToken = b.Configuration[BOT_TOKEN_KEY];
                ArgumentException.ThrowIfNullOrEmpty(botToken, nameof(botToken));
                TelegramBotClientOptions options = new(botToken);
                return new TelegramBotClient(options, c);
            }).RemoveAllLoggers();

            var host = b.Build();
            host.Run();
        }
    }
}