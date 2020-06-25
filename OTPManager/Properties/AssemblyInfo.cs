using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

// This attribute is used to give a name to this particular assembly.
[assembly: AssemblyTitle("OTPManager")]
// This attribute is used to describe the product that this particular assembly is for.
// Multiple assemblies can be components of the same product, in which case they can all share the same value for this attribute.
[assembly: AssemblyProduct("OTPManager")]
[assembly: AssemblyDescription("Time-based One-time Password generator manager.")]
[assembly: AssemblyCopyright("Daniil Shipilin (daniil.shipilin@gmail.com)")]
[assembly: AssemblyTrademark("Daniil Shipilin (daniil.shipilin@gmail.com)")]
// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("c8f7186f-6e1b-4e62-a821-7278cf94b915")]
[assembly: ComVisible(true)]

namespace OTPManager
{
    public static class AssemblyInfo
    {
        static readonly Assembly _ass = Assembly.GetExecutingAssembly();
        static readonly AssemblyTitleAttribute _title = _ass.GetCustomAttributes<AssemblyTitleAttribute>().FirstOrDefault();
        static readonly AssemblyProductAttribute _product = _ass.GetCustomAttributes<AssemblyProductAttribute>().FirstOrDefault();
        static readonly AssemblyDescriptionAttribute _description = _ass.GetCustomAttributes<AssemblyDescriptionAttribute>().FirstOrDefault();
        static readonly AssemblyCopyrightAttribute _copyright = _ass.GetCustomAttributes<AssemblyCopyrightAttribute>().FirstOrDefault();
        static readonly AssemblyTrademarkAttribute _trademark = _ass.GetCustomAttributes<AssemblyTrademarkAttribute>().FirstOrDefault();
        static readonly GuidAttribute _guid = _ass.GetCustomAttributes<GuidAttribute>().FirstOrDefault();

        public static string BaseDirectory { get; } = AppDomain.CurrentDomain.BaseDirectory;
        public static string AppPath { get; } = _ass.Location;
        public static string AppTitle { get; } = _title.Title;
        public static string AppHeader { get; } = $"{_title.Title} v{GitVersionInformation.SemVer}";
        public static string AppAuthor { get; } = _copyright.Copyright;
        public static string AppDescription { get; } = _description.Description;
        public static string AppGUID { get; } = _guid.Value;

        /// <summary>
        /// Formatted application info string.
        /// </summary>
        public static string AppInfoFormatted { get; } =
            $"{AppHeader}{Environment.NewLine}" +
            $"{GitVersionInformation.InformationalVersion}{Environment.NewLine}" +
            $"Author: {AppAuthor}{Environment.NewLine}" +
            $"{Environment.NewLine}" +
            $"Description:{Environment.NewLine}" +
            $"  {AppDescription}{Environment.NewLine}";
    }
}
