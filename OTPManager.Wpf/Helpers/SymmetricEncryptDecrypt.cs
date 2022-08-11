namespace OTPManager.Wpf.Helpers;

using System;
using System.Security.Cryptography;

public static class SymmetricEncryptDecrypt
{
    public static byte[] Encrypt(byte[] textBytes, byte[] key)
    {
        if (key.Length != 32)
        {
            throw new ArgumentException("Key byte array size is incorrect", nameof(key));
        }

        using var aes = Aes.Create();
        aes.BlockSize = 128;
        aes.Key = key;
        aes.IV = new byte[16];

        using var encryptor = aes.CreateEncryptor();
        byte[] cipherText = encryptor.TransformFinalBlock(textBytes, 0, textBytes.Length);

        return cipherText;
    }

    public static byte[] Decrypt(byte[] encryptedBytes, byte[] key)
    {
        if (key.Length != 32)
        {
            throw new ArgumentException("Key byte array size is incorrect", nameof(key));
        }

        using var aes = Aes.Create();
        aes.BlockSize = 128;
        aes.Key = key;
        aes.IV = new byte[16];

        using var decryptor = aes.CreateDecryptor();
        byte[] plainBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

        return plainBytes;
    }

    public static bool TryDecrypt(byte[] encryptedBytes, byte[] key)
    {
        using var aes = Aes.Create();
        aes.BlockSize = 128;
        aes.Key = key;
        aes.IV = new byte[16];

        using var decryptor = aes.CreateDecryptor();

        try
        {
            _ = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
