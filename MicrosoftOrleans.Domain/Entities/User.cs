using MicrosoftOrleans.Domain.Interfaces;

namespace MicrosoftOrleans.Domain.Entities;


public class User
{
    public int Id { get; private set; }
    public string UserName { get; private set; }
    public byte[] Password { get; private set; }
    public byte[] IV { get; private set; }
    public List<Address> Addresses { get; private set; } = new List<Address>();

    public static User Create(string userName, string password, IEncryptionService encryptionService)
    {
        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentNullException(nameof(userName));

        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentNullException(nameof(password));

        var encryptedPasswordInfo = encryptionService.Encrypt(password);

        var user = new User
        {
            UserName = userName,
            Password = encryptedPasswordInfo.EncryptedData,
            IV = encryptedPasswordInfo.IV
        };

        return user;
    }

    public void SetUserName(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentNullException(nameof(userName));

        UserName = userName;
    }

    public Address? FindAddress(string city, string street, string alley, int plaque)
        => Addresses.FirstOrDefault(x => x.City == city && x.Street == street && x.Alley == alley && x.Plaque == plaque);
}
