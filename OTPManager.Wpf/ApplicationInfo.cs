namespace OTPManager.Wpf;

using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

public static class ApplicationInfo
{
    private static readonly Assembly Ass = Assembly.GetExecutingAssembly();
    private static readonly AssemblyTitleAttribute? Title = Ass.GetCustomAttribute<AssemblyTitleAttribute>();
    private static readonly AssemblyProductAttribute? Product = Ass.GetCustomAttribute<AssemblyProductAttribute>();
    private static readonly AssemblyDescriptionAttribute? Description = Ass.GetCustomAttribute<AssemblyDescriptionAttribute>();
    private static readonly AssemblyCopyrightAttribute? Copyright = Ass.GetCustomAttribute<AssemblyCopyrightAttribute>();

    public const string AppBuild =
#if DEBUG
        " [Debug]";
#else
        "";
#endif

    public static IList<string>? Args { get; private set; }

    public static string BaseDirectory => Path.GetDirectoryName(ExePath) ?? string.Empty;

    public static string ExePath { get; } = Process.GetCurrentProcess().MainModule?.FileName ?? string.Empty;

    public static string AppTitle { get; } = Title?.Title ?? string.Empty;

    public static string AppProduct { get; } = Product?.Product ?? string.Empty;

    public static string AppHeader => $"{AppTitle} v{AppVersion.ToString(3)}{AppBuild}";

    public static Version AppVersion { get; } = Ass.GetName().Version!;

    public static string AppAuthor { get; } = Copyright?.Copyright ?? string.Empty;

    public static string AppDescription { get; } = Description?.Description ?? string.Empty;

    public static Guid AppGUID { get; } = new Guid("c8f7186f-6e1b-4e62-a821-7278cf94b915");

    /// <summary>
    /// Gets application info formatted string.
    /// </summary>
    public static string AppInfoFormatted =>
        $"{AppHeader}{Environment.NewLine}" +
        $"{AppVersion}{Environment.NewLine}" +
        $"Author: {AppAuthor}{Environment.NewLine}{Environment.NewLine}" +
        $"Description:{Environment.NewLine}" +
        $"  {AppDescription}";

    /// <summary>
    /// Sets application command line arguments.
    /// </summary>
    public static void SetArgs(string[] args) => Args = [.. args];
}
