using Orleans;
using System.Text.Json.Serialization;

namespace MicrosoftOrleans.Domain.Entities;

[GenerateSerializer]
public class Address
{
    [Id(0)]
    public required int Id { get; set; }

    [Id(1)]
    public required string Street { get; set; }

    [Id(2)]
    public required string City { get; set; }

    [Id(3)]
    public required int Plaque { get; set; }

    [Id(4)]
    public int UserId { get; set; }

    [Id(5)]
    [JsonIgnore]
    public User User { get; set; }
}
