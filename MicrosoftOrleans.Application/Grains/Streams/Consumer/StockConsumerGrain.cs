using MicrosoftOrleans.Application.Common.Interfaces;
using MicrosoftOrleans.Application.DTOs;
using Orleans.Streams;
using Serilog;

namespace MicrosoftOrleans.Application.Grains.Streams.Consumer;

public class StockConsumerGrain : Grain, IStockConsumerGrain
{
    private readonly List<StockTickDto> _history = new();
    private string _subscribedSymbol = string.Empty;

    public async Task SubscribeToSymbol(string symbol)
    {
        _subscribedSymbol = symbol;
        var streamProvider = this.GetStreamProvider("MemoryStream");
        var stream = streamProvider.GetStream<StockTickDto>(
            StreamId.Create("StockTicks", "GlobalStream"));

        await stream.SubscribeAsync((tick, _) =>
        {
            if (tick.Symbol == _subscribedSymbol)
            {
                _history.Add(tick);
                Log.Information($"CONSUMER {this.GetPrimaryKeyString()}: Received {tick.Symbol} @ {tick.Price}");
            }
            return Task.CompletedTask;
        });
    }

    public Task<List<StockTickDto>> GetHistory() => Task.FromResult(_history);

}
