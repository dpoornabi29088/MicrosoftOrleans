using System.Security.Cryptography;
using System.Text;

namespace MicrosoftOrleans.Infrastructure.Helpers.Security;

public static class SecurityHelper
{
    public static (byte[] EncryptedBytes, byte[] Key, byte[] IV) Encrypt(string plainText)
    {
        using Aes aes = Aes.Create();
        aes.GenerateKey();
        aes.GenerateIV();

        using ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
        byte[] encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        return (encryptedBytes, aes.Key, aes.IV); // Returning encrypted data, key, and IV
    }

    public static string Decrypt(byte[] encryptedData, byte[] key, byte[] iv)
    {
        using Aes aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;

        using ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedData, 0, encryptedData.Length);

        return Encoding.UTF8.GetString(decryptedBytes);
    }
}
