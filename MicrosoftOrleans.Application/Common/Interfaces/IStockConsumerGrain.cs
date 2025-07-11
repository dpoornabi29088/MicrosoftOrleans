
using MicrosoftOrleans.Application.DTOs;

namespace MicrosoftOrleans.Application.Common.Interfaces;

public interface IStockConsumerGrain : IGrainWithStringKey
{
    Task SubscribeToSymbol(string symbol);
    Task<List<StockTickDto>> GetHistory();
}
