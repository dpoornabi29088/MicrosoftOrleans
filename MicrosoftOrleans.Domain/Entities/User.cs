using Orleans;

namespace MicrosoftOrleans.Domain.Entities;

[GenerateSerializer]
public class User
{
    [Id(0)]
    public int Id { get; set; }

    [Id(1)]
    public required string UserName { get; set; }

    [Id(2)]
    public required byte Password { get; set; }

    [Id(3)]
    public required byte IV { get; set; }

    [Id(4)]
    public List<Address> Addresses { get; set; } = new List<Address>();
}
