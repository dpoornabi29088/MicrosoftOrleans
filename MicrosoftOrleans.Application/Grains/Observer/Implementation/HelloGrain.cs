using Microsoft.Extensions.Logging;
using Orleans.Utilities;

namespace MicrosoftOrleans.Application.Grains.Observer.Implementation;

public class HelloGrain : Grain, IHelloGrain
{
    private readonly ObserverManager<IChat> _subsManager;

    public HelloGrain(ILogger<HelloGrain> logger)
    {
        _subsManager = new ObserverManager<IChat>(TimeSpan.FromMinutes(5), logger);
    }

    // Clients call this to subscribe.
    public Task Subscribe(IChat observer)
    {
        _subsManager.Subscribe(observer, observer);

        return Task.CompletedTask;
    }

    //Clients use this to unsubscribe and no longer receive messages.
    public Task UnSubscribe(IChat observer)
    {
        _subsManager.Unsubscribe(observer);

        return Task.CompletedTask;
    }

    public Task SendUpdateMessage(string message)
    {
        _subsManager.Notify(s => s.ReceiveMessage(message));

        return Task.CompletedTask;
    }
}