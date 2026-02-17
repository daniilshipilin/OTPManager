namespace OTPManager.Wpf.Helpers;

using System;
using System.Threading.Tasks;
using GuerrillaNtp;

public static class NtpTimeProvider
{
    private static readonly NtpClient ntpClient = NtpClient.Default;
    private static DateTimeOffset? lastSuccessfulSyncTime;

    public static async Task InitializeAsync()
    {
        try
        {
            var ntpClock = await ntpClient.QueryAsync();
            lastSuccessfulSyncTime = ntpClock.UtcNow;
        }
        catch (Exception)
        {
        }
    }

    public static DateTime GetAccurateUtcNow()
    {
        if (ntpClient.Last is not null && lastSuccessfulSyncTime.HasValue &&
            (DateTimeOffset.UtcNow - lastSuccessfulSyncTime.Value) < TimeSpan.FromHours(1))
        {
            return ntpClient.Last.UtcNow.DateTime;
        }

        try
        {
            var ntpClock = ntpClient.Query();
            lastSuccessfulSyncTime = ntpClock.UtcNow;
            return ntpClock.UtcNow.DateTime;
        }
        catch (Exception)
        {
            return DateTime.UtcNow;
        }
    }
}
