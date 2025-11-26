using MicrosoftOrleans.Application.Common.Interfaces;
using MicrosoftOrleans.Application.DTOs;
using Orleans.Streams;

namespace MicrosoftOrleans.Application.Grains.Streams.Producer;

public class StockProducerGrain : Grain, IStockProducerGrain
{
    private IStreamProvider _streamProvider;
    private readonly Random _random = new();

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        _streamProvider = this.GetStreamProvider("MemoryStream");
        return Task.CompletedTask;
    }

    public async Task PublishUpdate(string symbol)
    {
        var stream = _streamProvider.GetStream<StockTickDto>(
            StreamId.Create("STOCKS", "GlobalStream"));

        var tick = new StockTickDto(
            symbol,
            Math.Round(100 + (decimal)(_random.NextDouble() * 10), 2),
            DateTime.UtcNow);

        await stream.OnNextAsync(tick);

        Console.WriteLine($"PRODUCER: Sent {tick.Symbol} @ {tick.Price}");
    }
}
