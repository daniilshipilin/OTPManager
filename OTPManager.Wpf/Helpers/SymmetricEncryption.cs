namespace OTPManager.Wpf.Helpers;

using System;
using System.IO;
using System.Security.Cryptography;

public class SymmetricEncryption : IDisposable
{
    private readonly string IvKeyPath = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath)!, "iv.key");
    private readonly byte[] iv;
    private readonly byte[] key;
    private readonly ICryptoTransform encryptor;
    private readonly ICryptoTransform decryptor;

    public SymmetricEncryption(byte[] key)
    {
        if (key.Length != 32)
        {
            throw new ArgumentException("Key byte array size is incorrect", nameof(key));
        }

        if (!File.Exists(this.IvKeyPath))
        {
            byte[] data = new byte[16];
            RandomNumberGenerator.Fill(data);
            using var fs = new FileStream(this.IvKeyPath, FileMode.CreateNew);
            fs.Write(data);
        }

        this.key = key;
        this.iv = File.ReadAllBytes(this.IvKeyPath);

        (this.encryptor, this.decryptor) = this.GetCrypto();
    }

    public void Dispose()
    {
        this.encryptor.Dispose();
        this.decryptor.Dispose();
    }

    public byte[] Encrypt(byte[] data)
        => this.encryptor.TransformFinalBlock(data, 0, data.Length);

    public byte[] Decrypt(byte[] data)
        => this.decryptor.TransformFinalBlock(data, 0, data.Length);

    public bool TryDecrypt(byte[] encryptedBytes, out byte[]? plainBytes)
    {
        plainBytes = null;

        try
        {
            plainBytes = this.Decrypt(encryptedBytes);
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }

    private (ICryptoTransform Encryptor, ICryptoTransform Decryptor) GetCrypto()
    {
        using var aes = Aes.Create();
        aes.BlockSize = 128;
        aes.Key = this.key;
        aes.IV = this.iv;

        var encryptor = aes.CreateEncryptor();
        var decryptor = aes.CreateDecryptor();

        return (encryptor, decryptor);
    }
}
