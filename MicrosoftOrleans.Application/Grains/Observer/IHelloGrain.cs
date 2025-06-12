namespace MicrosoftOrleans.Application.Grains.Observer;

public interface IHelloGrain : IGrainWithIntegerKey
{
    Task Subscribe(IChat observer);
    Task UnSubscribe(IChat observer);
    Task SendUpdateMessage(string message);
}
