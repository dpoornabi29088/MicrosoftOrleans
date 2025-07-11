namespace MicrosoftOrleans.Application.Common.Interfaces;

public interface IStockProducerGrain : IGrainWithStringKey
{
    Task PublishUpdate(string symbol);
}
