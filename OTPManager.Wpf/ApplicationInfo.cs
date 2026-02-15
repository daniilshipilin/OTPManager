namespace OTPManager.Wpf;

using System.Reflection;

public static class ApplicationInfo
{
    private static readonly Assembly Ass = Assembly.GetExecutingAssembly();
    private static readonly AssemblyTitleAttribute? Title = Ass.GetCustomAttribute<AssemblyTitleAttribute>();
    private static readonly AssemblyFileVersionAttribute? FileVersion = Ass.GetCustomAttribute<AssemblyFileVersionAttribute>();

    public const string AppBuild =
#if DEBUG
        " [Debug]";
#else
        "";
#endif

    public static string AppHeader => $"{Title?.Title} v{FileVersion?.Version}{AppBuild}";
}
