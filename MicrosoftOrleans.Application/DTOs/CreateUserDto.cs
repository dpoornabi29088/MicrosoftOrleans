using MicrosoftOrleans.Domain.Entities;
using Orleans;

namespace MicrosoftOrleans.Application.DTOs;

[GenerateSerializer]
public class CreateUserDto
{
    [Id(0)]
    public required string UserName { get; set; }

    [Id(1)]
    public required byte Password { get; set; }

    [Id(2)]
    public required byte IV { get; set; }

    public static implicit operator User(CreateUserDto dto)
    {
        return new User
        {
            UserName = dto.UserName,
            Password = dto.Password,
            IV = dto.IV
        };
    }
}
