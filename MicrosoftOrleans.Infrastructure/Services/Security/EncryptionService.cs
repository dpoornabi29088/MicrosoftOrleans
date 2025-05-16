using MicrosoftOrleans.Domain.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace MicrosoftOrleans.Infrastructure.Services.Security;

public class EncryptionService : IEncryptionService
{
    public static readonly byte[] Key = Encoding.UTF8.GetBytes("&^%*$#@!fDavoudP@urnabi1236%$#@!"); // Must be exactly 32 chars (or 32 bytes)

    public (byte[] EncryptedData, byte[] IV) Encrypt(string plainText)
    {
        using Aes aes = Aes.Create();
        aes.Key = Key;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
        byte[] encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        return (encryptedBytes, aes.IV);
    }

    public string Decrypt(byte[] encryptedData, byte[] iv)
    {
        using Aes aes = Aes.Create();
        aes.Key = Key;
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedData, 0, encryptedData.Length);

        return Encoding.UTF8.GetString(decryptedBytes);
    }
}