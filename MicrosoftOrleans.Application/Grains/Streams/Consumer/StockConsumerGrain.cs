using MicrosoftOrleans.Application.Common.Interfaces;
using MicrosoftOrleans.Application.DTOs;
using Orleans.Streams;
using Serilog;

namespace MicrosoftOrleans.Application.Grains.Streams.Consumer;

//Note: We don't need this consumer because we implement consumer in UserGrain
//There are two ways to write consumer : the first one is like this class 
// The second one implement in your grain like UserGrain as you see

[ImplicitStreamSubscription("STOCKS")]
public class StockConsumerGrain : Grain, IStockConsumerGrain, IAsyncObserver<StockTickDto>
{
    private readonly List<StockTickDto> _history = new();
    private readonly HashSet<StreamSequenceToken> _receivedTokens = new();//Idempotency handler
    private readonly int _maxTokens = 1000;

    public override async Task OnActivateAsync(CancellationToken token)
    {
        var symbol = this.GetPrimaryKeyString();

        var provider = this.GetStreamProvider("MemoryStream");

        var stream = provider.GetStream<StockTickDto>(
            StreamId.Create("STOCKS", "GlobalStream"));

        await stream.SubscribeAsync(this);

        Log.Information($"CONSUMER {symbol}: Subscribed implicitly to STOCKS/{symbol}");
    }

    public Task OnNextAsync(StockTickDto tick, StreamSequenceToken? token)
    {
        if (token == null || _receivedTokens.Contains(token))
        {
            Log.Warning($"Duplicate or null token skipped {tick.Symbol} @ {tick.Price}");
            return Task.CompletedTask;
        }

        _history.Add(tick);

        _receivedTokens.Add(token);

        if (_receivedTokens.Count > _maxTokens)
            _receivedTokens.Clear();

        Log.Information($"CONSUMER {this.GetPrimaryKeyString()}: Received {tick.Symbol} @ {tick.Price}");

        return Task.CompletedTask;
    }

    public Task OnErrorAsync(Exception ex)
    {
        Log.Error(ex, $"Stream error in CONSUMER {this.GetPrimaryKeyString()}");
        return Task.CompletedTask;
    }

    public Task OnCompletedAsync()
    {
        Log.Warning($"Stream completed for CONSUMER {this.GetPrimaryKeyString()}");
        return Task.CompletedTask;
    }

    public Task<List<StockTickDto>> GetHistory()
        => Task.FromResult(_history);

    public Task ClearHistory()
    {
        _history.Clear();
        Log.Information($"CONSUMER {this.GetPrimaryKeyString()}: History cleared.");
        return Task.CompletedTask;
    }
}
