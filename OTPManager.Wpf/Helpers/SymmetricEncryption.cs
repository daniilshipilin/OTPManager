namespace OTPManager.Wpf.Helpers;

using System;
using System.Security.Cryptography;

public class SymmetricEncryption : IDisposable
{
    private readonly byte[] key;
    private readonly ICryptoTransform encryptor;
    private readonly ICryptoTransform decryptor;

    private bool disposedValue;

    public SymmetricEncryption()
        : this(new byte[32])
    {
    }

    public SymmetricEncryption(byte[] key)
    {
        if (key.Length != 32)
        {
            throw new ArgumentException("Key byte array size is incorrect", nameof(key));
        }

        this.key = key;

        (this.encryptor, this.decryptor) = this.GetCrypto();
    }

    public byte[] Encrypt(byte[] textBytes)
    {
        byte[] cipherText = this.encryptor.TransformFinalBlock(textBytes, 0, textBytes.Length);

        return cipherText;
    }

    public byte[] Decrypt(byte[] encryptedBytes)
    {
        byte[] plainBytes = this.decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

        return plainBytes;
    }

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
        aes.IV = new byte[16];

        var encryptor = aes.CreateEncryptor();
        var decryptor = aes.CreateDecryptor();

        return (encryptor, decryptor);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!this.disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
                this.encryptor.Dispose();
                this.decryptor.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            this.disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
