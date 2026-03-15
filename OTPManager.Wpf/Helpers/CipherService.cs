namespace OTPManager.Wpf.Helpers;

using System.IO;
using System.Security.Cryptography;

public class CipherService(byte[] password)
{
    private const int Iterations = 1_000_000;
    private const int KeySize = 32;

    public byte[] Encrypt(string plainText)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        byte[] key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, KeySize);

        using var aes = Aes.Create();
        aes.GenerateIV();
        aes.Key = key;

        using var ms = new MemoryStream();

        // Salt (16) + IV (16) + Ciphertext
        ms.Write(salt);
        ms.Write(aes.IV);

        using var encryptor = aes.CreateEncryptor();
        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        using (var sw = new StreamWriter(cs))
        {
            sw.Write(plainText);
        }

        return ms.ToArray();
    }

    public string Decrypt(byte[] cipherText)
    {
        // Salt = [0-15], IV = [16-31], Ciphertext = [32-End]
        byte[] salt = cipherText[..16];
        byte[] iv = cipherText[16..32];
        byte[] encryptedData = cipherText[32..];
        byte[] key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, KeySize);

        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;

        using var ms = new MemoryStream(encryptedData);
        using var decryptor = aes.CreateDecryptor();
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);

        return sr.ReadToEnd();
    }

    public bool TryDecrypt(byte[] cipherText, out string? plainText)
    {
        try
        {
            plainText = this.Decrypt(cipherText);
            return true;
        }
        catch (CryptographicException)
        {
            plainText = null;
            return false;
        }
    }
}
