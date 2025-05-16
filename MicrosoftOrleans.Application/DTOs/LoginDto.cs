using Orleans;

namespace MicrosoftOrleans.Application.DTOs;

[GenerateSerializer]
public class LoginDto
{
    [Id(0)]
    public required string UserName { get; set; }

    [Id(1)]
    public required string Password { get; set; }
}
