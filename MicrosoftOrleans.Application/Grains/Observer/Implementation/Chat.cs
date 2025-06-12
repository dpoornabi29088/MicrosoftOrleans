namespace MicrosoftOrleans.Application.Grains.Observer.Implementation;

public class Chat : IChat
{
    public Task ReceiveMessage(string message)
    {
        Console.WriteLine(message);
        return Task.CompletedTask;
    }
}
