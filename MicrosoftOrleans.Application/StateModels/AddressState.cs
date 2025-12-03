namespace MicrosoftOrleans.Application.StateModels;

public class AddressState
{
    [Id(0)]
    public int Id { get; set; }

    [Id(1)]
    public string Street { get; set; }

    [Id(2)]
    public string City { get; set; }

    [Id(3)]
    public string Alley { get; set; }

    [Id(4)]
    public int Plaque { get; set; }

    [Id(5)]
    public int UserId { get; set; }

    [Id(6)]
    public UserState User { get; set; }
}