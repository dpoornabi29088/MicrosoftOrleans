using MicrosoftOrleans.Application.Common.Interfaces;
using MicrosoftOrleans.Application.DTOs;
using Orleans.Streams;

namespace MicrosoftOrleans.Application.Grains.Streams.Producer;

public class StockProducerGrain : Grain, IStockProducerGrain
{
    private IAsyncStream<StockTickDto> _stream;
    private readonly Random _random = new();

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        var streamProvider = this.GetStreamProvider("MemoryStream");
        _stream = streamProvider.GetStream<StockTickDto>(
            StreamId.Create("StockTicks", "GlobalStream"));
        return Task.CompletedTask;
    }

    public async Task PublishUpdate(string symbol)
    {
        var tick = new StockTickDto(
            symbol,
            Math.Round(100 + (decimal)(_random.NextDouble() * 10), 2),
            DateTime.UtcNow);

        await _stream.OnNextAsync(tick);
        Console.WriteLine($"PRODUCER: Sent {tick.Symbol} @ {tick.Price}");
    }
}
