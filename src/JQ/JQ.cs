using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.Buffered;

namespace Devlooped;

/// <summary>
/// Executes JQ queries on JSON input.
/// </summary>
/// <remarks>
/// Learn more about JQ at https://jqlang.github.io/jq/.
/// </remarks>
public static class JQ
{
    static readonly string jqpath;

    static JQ()
    {
        jqpath =
            OperatingSystem.IsLinux() && RuntimeInformation.ProcessArchitecture == Architecture.X64 ?
            System.IO.Path.Combine(AppContext.BaseDirectory, "lib", "jq-linux-amd64") :
            OperatingSystem.IsLinux() && RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ?
            System.IO.Path.Combine(AppContext.BaseDirectory, "lib", "jq-linux-arm64") :
            OperatingSystem.IsLinux() && RuntimeInformation.ProcessArchitecture == Architecture.X86 ?
            System.IO.Path.Combine(AppContext.BaseDirectory, "lib", "jq-linux-i386") :
            OperatingSystem.IsWindows() && RuntimeInformation.ProcessArchitecture == Architecture.X64 ?
            System.IO.Path.Combine(AppContext.BaseDirectory, "lib", "jq-windows-amd64.exe") :
            OperatingSystem.IsWindows() && RuntimeInformation.ProcessArchitecture == Architecture.X86 ?
            System.IO.Path.Combine(AppContext.BaseDirectory, "lib", "jq-windows-i386.exe") :
            OperatingSystem.IsMacOS() && RuntimeInformation.ProcessArchitecture == Architecture.X64 ?
            System.IO.Path.Combine(AppContext.BaseDirectory, "lib", "jq-macos-amd64") :
            OperatingSystem.IsMacOS() && RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ?
            System.IO.Path.Combine(AppContext.BaseDirectory, "lib", "jq-macos-arm64") :
            throw new PlatformNotSupportedException("Unsupported platform or architecture.");

        Debug.Assert(File.Exists(jqpath));

#if NET8_0_OR_GREATER
        if (!OperatingSystem.IsWindows())
            File.SetUnixFileMode(jqpath, UnixFileMode.UserExecute);
#endif
    }

    /// <summary>
    /// Gets the path to the JQ executable for more flexible execution, if needed.
    /// </summary>
    public static string Path => jqpath;

    /// <summary>
    /// Execute the query and return the selected result.
    /// </summary>
    public static async Task<string> ExecuteAsync(string json, string query)
    {
        if (!File.Exists(jqpath))
            throw new FileNotFoundException($"JQ executable not found.", jqpath);

        var jq = await Cli.Wrap(jqpath)
            .WithArguments(["-r", query.ReplaceLineEndings().Replace(Environment.NewLine, " ")])
            .WithStandardInputPipe(PipeSource.FromString(json, Encoding.UTF8))
            .WithValidation(CommandResultValidation.None)
            .ExecuteBufferedAsync();

        return jq.StandardOutput.Trim();
    }
}
