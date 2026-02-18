namespace OTPManager.Wpf.Helpers;

using System;
using System.Threading.Tasks;
using GuerrillaNtp;

public static class NtpTimeProvider
{
    private static readonly NtpClient ntpClient = NtpClient.Default;

    public static async Task InitializeAsync()
    {
        try
        {
            await Task.Delay(5000);
            await ntpClient.QueryAsync();
        }
        catch (Exception)
        {
        }
    }

    public static DateTime GetAccurateUtcNow()
        => ntpClient.Last is not null
            ? ntpClient.Last.UtcNow.DateTime
            : throw new NullReferenceException(nameof(ntpClient.Last));
}
