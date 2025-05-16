namespace MicrosoftOrleans.Domain.Interfaces;

public interface IEncryptionService
{
    (byte[] EncryptedData, byte[] IV) Encrypt(string plainText);
    string Decrypt(byte[] encryptedData, byte[] iv);
}
