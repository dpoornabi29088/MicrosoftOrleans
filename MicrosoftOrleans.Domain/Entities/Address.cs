using Orleans;

namespace MicrosoftOrleans.Domain.Entities;

[GenerateSerializer]
public class Address
{
    [Id(0)]
    public int Id { get; set; }

    [Id(1)]
    public string Street { get; set; }

    [Id(2)]
    public string City { get; set; }

    [Id(3)]
    public int Plaququ { get; set; }

    [Id(4)]
    public long UserId { get; set; }
}
