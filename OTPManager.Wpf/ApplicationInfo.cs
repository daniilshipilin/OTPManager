namespace OTPManager.Wpf;

using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

public static class ApplicationInfo
{
    private static readonly Assembly Ass = Assembly.GetExecutingAssembly();
    private static readonly AssemblyTitleAttribute? Title = Ass.GetCustomAttributes<AssemblyTitleAttribute>().FirstOrDefault();
    private static readonly AssemblyProductAttribute? Product = Ass.GetCustomAttributes<AssemblyProductAttribute>().FirstOrDefault();
    private static readonly AssemblyDescriptionAttribute? Description = Ass.GetCustomAttributes<AssemblyDescriptionAttribute>().FirstOrDefault();
    private static readonly AssemblyCopyrightAttribute? Copyright = Ass.GetCustomAttributes<AssemblyCopyrightAttribute>().FirstOrDefault();

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

    public static string AppHeader { get; } = $"{AppTitle} v{GitVersionInformation.SemVer}{AppBuild}";

    public static string AppAuthor { get; } = Copyright?.Copyright ?? string.Empty;

    public static string AppDescription { get; } = Description?.Description ?? string.Empty;

    public static Guid AppGUID { get; } = new Guid("c8f7186f-6e1b-4e62-a821-7278cf94b915");

    /// <summary>
    /// Gets application info formatted string.
    /// </summary>
    public static string AppInfoFormatted =>
        $"{AppHeader}{Environment.NewLine}" +
        $"{GitVersionInformation.InformationalVersion}{Environment.NewLine}" +
        $"Author: {AppAuthor}{Environment.NewLine}{Environment.NewLine}" +
        $"Description:{Environment.NewLine}" +
        $"  {AppDescription}";

    /// <summary>
    /// Sets application command line arguments.
    /// </summary>
    public static void SetArgs(string[] args) => Args = args.ToList();
}
