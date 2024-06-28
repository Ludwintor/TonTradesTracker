using TonSharp.Core;

namespace TradesTracker.Dedust
{
    public interface IDedustClient
    {
        Task<DedustAsset> GetAssetAsync(Address address);
        Task<Trade[]> GetTradesAsync(Address poolAddress, int count, UInt128? afterLt = null);
        Task UpdatePoolAsync(DedustPool pool);
    }
}