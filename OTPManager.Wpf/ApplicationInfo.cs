namespace OTPManager.Wpf;

using System;
using System.Diagnostics;
using System.Reflection;

public static class ApplicationInfo
{
    private static readonly Assembly Ass = Assembly.GetExecutingAssembly();
    private static readonly AssemblyTitleAttribute? Title = Ass.GetCustomAttribute<AssemblyTitleAttribute>();

    public const string AppBuild =
#if DEBUG
        " [Debug]";
#else
        "";
#endif

    public static string ExePath { get; } = Process.GetCurrentProcess().MainModule?.FileName ?? string.Empty;

    public static string AppTitle { get; } = Title?.Title ?? string.Empty;

    public static string AppHeader => $"{AppTitle} v{AppVersion.ToString(3)}{AppBuild}";

    public static Version AppVersion { get; } = Ass.GetName().Version!;
}
