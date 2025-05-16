using Orleans;

namespace MicrosoftOrleans.Application.DTOs;

[GenerateSerializer]
public record LoginDto(string UserName, string Password);
