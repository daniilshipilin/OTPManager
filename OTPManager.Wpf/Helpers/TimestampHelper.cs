namespace OTPManager.Wpf.Helpers;

using System;

public static class TimestampHelper
{
    public static int GetUnixTimestamp() => (int)((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds();

    public static DateTime UnixTimeStampToDateTime(int unixTimeStamp)
    {
        var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        return dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
    }
}
