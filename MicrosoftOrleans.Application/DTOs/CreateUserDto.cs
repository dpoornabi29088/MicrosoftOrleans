using Orleans;

namespace MicrosoftOrleans.Application.DTOs;

[GenerateSerializer]
public record CreateUserDto(string UserName, string Password);
