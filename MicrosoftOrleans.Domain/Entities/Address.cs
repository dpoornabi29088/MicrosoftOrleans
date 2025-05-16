using Orleans;
using System.Text.Json.Serialization;

namespace MicrosoftOrleans.Domain.Entities;

[GenerateSerializer]
public class Address
{
    [Id(0)]
    public int Id { get; private set; }

    [Id(1)]
    public string Street { get; private set; }

    [Id(2)]
    public string City { get; private set; }

    [Id(3)]
    public string Alley { get; private set; }

    [Id(4)]
    public int Plaque { get; private set; }

    [Id(5)]
    public int UserId { get; private set; }

    [Id(6)]
    [JsonIgnore]
    public User User { get; private set; }

    public static Address Create(int userId, string city, string street, string alley, int plaque)
    {
        if (userId <= 0)
            throw new ArgumentException("The userId must be greater than zero");

        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentNullException(nameof(city));

        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentNullException(nameof(Street));

        if (string.IsNullOrWhiteSpace(alley))
            throw new ArgumentNullException(nameof(alley));

        if (plaque <= 0)
            throw new ArgumentNullException("The plaque must be greater than zero");

        var address = new Address
        {
            UserId = userId,
            City = city,
            Street = street,
            Alley = alley,
            Plaque = plaque
        };

        return address;
    }

    public void SetCity(string city)
    {
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentNullException(nameof(city));

        City = city;
    }

    public void SetStreet(string street)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentNullException(nameof(street));

        Street = street;
    }

    public void SetAlley(string alley)
    {
        if (string.IsNullOrWhiteSpace(alley))
            throw new ArgumentNullException(nameof(alley));

        Alley = alley;
    }

    public void SetPlaque(int plaque)
    {
        if (plaque <= 0)
            throw new ArgumentNullException("The plaque must be greater than zero");

        Plaque = plaque;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Address other) return false;
        return City == other.City && Street == other.Street && Alley == other.Alley && Plaque == other.Plaque;
    }

    public override int GetHashCode() => HashCode.Combine(City, Street, Alley, Plaque);
}
