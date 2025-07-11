namespace MicrosoftOrleans.Application.DTOs;

[GenerateSerializer]
public record StockTickDto(string Symbol, decimal Price, DateTime Timestamp);