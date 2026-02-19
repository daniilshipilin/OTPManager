namespace OTPManager.Wpf.Helpers;

using System;
using System.Net.Http;
using System.Threading.Tasks;

public static class NetworkTimeProvider
{
    public static async Task<DateTimeOffset> GetNetworkTimeAsync()
    {
        try
        {
            using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            var response = await httpClient.GetAsync("https://1.1.1.1", HttpCompletionOption.ResponseHeadersRead);

            if (response.Headers.Date.HasValue)
            {
                return response.Headers.Date.Value;
            }
        }
        catch (Exception)
        {
        }

        return DateTimeOffset.UtcNow;
    }
}
