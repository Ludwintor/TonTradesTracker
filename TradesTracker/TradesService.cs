using System.Text;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TonSharp.Core;
using TradesTracker.Dedust;
using TradesTracker.Options;
using TradesTracker.Tonapi;

namespace TradesTracker
{
    public class TradesService : BackgroundService
    {
        private readonly ILogger<TradesService> _logger;
        private readonly ITelegramBotClient _bot;
        private readonly IDedustClient _dedust;
        private readonly ITonapiClient _tonapi;
        private readonly TrackerOptions _options;
        private readonly Address _tokenAddress;
        private readonly Address _poolAddress;
        private InlineKeyboardMarkup _buttons;

        private UInt128 _lastLt;
        private double _lastPrice;

        public TradesService(ILogger<TradesService> logger, ITelegramBotClient bot,
            IDedustClient dedust, ITonapiClient tonapi, IOptions<TrackerOptions> trackerOptions)
        {
            _logger = logger;
            _bot = bot;
            _dedust = dedust;
            _tonapi = tonapi;

            _options = trackerOptions.Value;
            _tokenAddress = Address.ParseRaw(_options.TokenAddress);
            _poolAddress = Address.ParseRaw(_options.PoolAddress);

            _buttons = new([
                InlineKeyboardButton.WithUrl($"{Emojis.MoneyBag}Buy on Dedust", $"https://dedust.io/swap/TON/{_tokenAddress}"),
                InlineKeyboardButton.WithUrl($"{Emojis.BarChart}Chart", $"https://geckoterminal.com/ton/pools/{_poolAddress}")
            ]);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Bot {Name} started with channel {Id}", (await _bot.GetMeAsync(stoppingToken)).Username, _options.ChannelId);
            StringBuilder sb = new();
            TimeSpan passDelay = TimeSpan.FromSeconds(_options.PassDelay);
            DedustAsset tokenAsset = await _dedust.GetAssetAsync(_tokenAddress);
            _logger.LogInformation("Token metadata inited. Symbol: {Symbol} | Decimals: {Decimals}", tokenAsset.Symbol, tokenAsset.Decimals);
            DedustPool pool = new(_poolAddress, DedustAsset.Ton, tokenAsset);
            _lastLt = (await _dedust.GetTradesAsync(_poolAddress, 1)).First().Lt;
            await _dedust.UpdatePoolAsync(pool);
            _lastPrice = pool.PricePerRight;
            _logger.LogInformation("Pool TON/{Symbol} found: {Address} Price: {Price:0.000000} TON", tokenAsset.Symbol, pool.Address, _lastPrice);
            await Task.Delay(1000, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (await ProcessTracking(sb, pool, tokenAsset))
                        await _bot.MakeRequestAsync(new SendMessageRequest
                        {
                            ChatId = _options.ChannelId,
                            Text = sb.ToString(),
                            ReplyMarkup = _buttons,
                            ParseMode = ParseMode.MarkdownV2,
                            LinkPreviewOptions = new() { IsDisabled = true }
                        }, stoppingToken);
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, "Tracking failed with error: {Message}", ex.Message);
                }
                await Task.Delay(passDelay, stoppingToken);
            }
        }

        private async Task<bool> ProcessTracking(StringBuilder sb, DedustPool pool, DedustAsset tokenAsset)
        {
            Task poolTask = _dedust.UpdatePoolAsync(pool);
            Trade[] trades = await _dedust.GetTradesAsync(pool.Address, _options.TradesPerPass, _lastLt);
            _logger.LogInformation("Pool TON/{Symbol} found {Count} new trades", tokenAsset.Symbol, trades.Length);
            await poolTask;
            if (trades.Length == 0)
                return false;

            sb.Clear();
            TokenRates tonRate = await _tonapi.GetTonUsdRate();
            WriteTrades(sb, pool, trades, tonRate);

            double tonPrice = pool.PricePerRight;
            double usdPrice = tonPrice * tonRate.Price.Usd;
            double priceChange = tonPrice / _lastPrice - 1d;
            bool isUptrend = priceChange >= 0d;
            string chartEmoji = isUptrend ? Emojis.UptrendChart : Emojis.DowntrendChart;
            sb.AppendLine().Append(Emojis.BarChart)
              .Append(Utils.EscapeMarkdown(tonPrice.ToString("0.000000")))
              .Append(" TON \\($")
              .Append(Utils.EscapeMarkdown(usdPrice.ToString("0.000000")))
              .Append("\\) ").Append(chartEmoji)
              .Append(" \\").Append(isUptrend ? '+' : '-')
              .Append(Utils.EscapeMarkdown(Math.Abs(priceChange * 100d).ToString("0.00")))
              .Append('%');

            TokenRates tokenRate = await _tonapi.GetJettonRates(_tokenAddress);
            sb.AppendLine().Append(Emojis.Calendar)
              .Append("24h ").Append(Utils.EscapeMarkdown(tokenRate.DailyDiff.Ton)).Append(" \\| ")
              .Append("7d ").Append(Utils.EscapeMarkdown(tokenRate.WeeklyDiff.Ton)).Append(" \\| ")
              .Append("30d ").Append(Utils.EscapeMarkdown(tokenRate.MonthlyDiff.Ton));
            
            _lastPrice = tonPrice;
            _lastLt = trades[^1].Lt;
            return true;
        }

        private void WriteTrades(StringBuilder sb, DedustPool pool, IEnumerable<Trade> trades, TokenRates tonRate)
        {
            foreach (Trade trade in trades)
            {
                bool isBuy = trade.AssetOut.Type == AssetType.Jetton && Address.ParseFriendly(trade.AssetOut.Address).Equals(_tokenAddress);
                string buySellEmoji = isBuy ? Emojis.GreenDot : Emojis.RedDot;
                string arrowEmoji = isBuy ? "<<" : "\\>\\>";
                string buySell = isBuy ? "BUY" : "SELL";
                double ton = (double)(isBuy ? trade.AmountIn : trade.AmountOut) / Math.Pow(10d, pool.Left.Decimals);
                double jetton = (double)(isBuy ? trade.AmountOut : trade.AmountIn) / Math.Pow(10d, pool.Right.Decimals);

                // :RED_DOT:SELL 500 TOKEN >> 5 TON ($2.50) EQAA...FOO_
                sb.Append(buySellEmoji).Append(buySell).Append(' ')
                  .Append(Utils.EscapeMarkdown(jetton.ToString("0.00"))).Append(' ').Append(pool.Right.Symbol)
                  .Append(' ').Append(arrowEmoji).Append(' ')
                  .Append(Utils.EscapeMarkdown(ton.ToString("0.00"))).Append(' ').Append(pool.Left.Symbol);

                double usdPrice = ton * tonRate.Price.Usd;
                sb.Append(" \\($").Append(Utils.EscapeMarkdown(usdPrice.ToString("0.00"))).Append("\\)")
                  .Append(" [").Append(Utils.ShortAddress(trade.Sender, true)).Append("](")
                  .Append(_options.ExplorerUrl).Append(Utils.EscapeMarkdown(trade.Sender)).Append(')').AppendLine();
            }
        }
    }
}
