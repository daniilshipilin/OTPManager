namespace OTPManager.Wpf;

using System.Reflection;

public static class ApplicationInfo
{
    private static readonly Assembly Ass = Assembly.GetExecutingAssembly();
    private static readonly AssemblyTitleAttribute? Title = Ass.GetCustomAttribute<AssemblyTitleAttribute>();
    private static readonly AssemblyInformationalVersionAttribute? InformationalVersion = Ass.GetCustomAttribute<AssemblyInformationalVersionAttribute>();

    private const string AppBuild =
#if DEBUG
        " [Debug]";
#else
        "";
#endif

    public static string? AppTitle => Title?.Title;

    public static string AppHeader => $"{AppTitle} v{InformationalVersion?.InformationalVersion}{AppBuild}";
}
