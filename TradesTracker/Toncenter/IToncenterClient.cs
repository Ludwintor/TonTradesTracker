using TonSharp.Core;

namespace TradesTracker.Toncenter
{
    public interface IToncenterClient
    {
        Task<RunGetMethodResult> RunGetMethodAsync(Address address, string method, StackEntry[]? stack = null);
    }
}