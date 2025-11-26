
using MicrosoftOrleans.Application.DTOs;

namespace MicrosoftOrleans.Application.Common.Interfaces;

public interface IStockConsumerGrain : IGrainWithStringKey
{
    Task<List<StockTickDto>> GetHistory();
    Task ClearHistory();
}
