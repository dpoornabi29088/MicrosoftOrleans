namespace MicrosoftOrleans.Application.StateModels;

[GenerateSerializer]
public class UserState
{
    [Id(0)]
    public int Id { get; set; }

    [Id(1)]
    public string UserName { get; set; }

    [Id(2)]
    public byte[] Password { get; set; }

    [Id(3)]
    public byte[] IV { get; set; }

    [Id(4)]
    public List<AddressState> Addresses { get; set; } = new List<AddressState>();
}
