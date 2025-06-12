namespace MicrosoftOrleans.Application.Grains.Observer;

public interface IChat : IGrainObserver
{
    Task ReceiveMessage(string message);
}
