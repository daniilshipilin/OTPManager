namespace OTPManager.Wpf.Helpers;

using System;
using GuerrillaNtp;

public static class NtpTimeProvider
{
    private static NtpClock? cachedNtpClock;
    private static DateTimeOffset? lastSuccessfulSyncTime;

    public static DateTimeOffset GetAccurateUtcNow()
    {
        if (cachedNtpClock is not null && lastSuccessfulSyncTime.HasValue &&
            (DateTimeOffset.UtcNow - lastSuccessfulSyncTime.Value) < TimeSpan.FromHours(1))
        {
            return cachedNtpClock.UtcNow;
        }

        try
        {
            var client = NtpClient.Default;
            cachedNtpClock = client.Query();
            lastSuccessfulSyncTime = DateTimeOffset.UtcNow;
            return cachedNtpClock.UtcNow;
        }
        catch (Exception)
        {
            return DateTimeOffset.UtcNow;
        }
    }
}
