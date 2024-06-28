
using TonSharp.Core;

namespace TradesTracker.Tonapi
{
    public interface ITonapiClient
    {
        Task<TokenRates> GetTonUsdRate();
        Task<TokenRates> GetJettonRates(Address address);
    }
}