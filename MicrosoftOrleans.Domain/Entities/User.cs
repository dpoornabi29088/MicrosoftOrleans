using Orleans;

namespace MicrosoftOrleans.Domain.Entities;

[GenerateSerializer]
public sealed class User
{
    [Id(0)]
    public int Id { get; set; }

    [Id(1)]
    public required string UserName { get; set; }

    [Id(2)]
    public required string Password { get; set; }

    [Id(3)]
    public List<Address> Addresses { get; set; } = new List<Address>();
}
