using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
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

        var normalized = query.ReplaceLineEndings().Trim();
        if (normalized.Contains(Environment.NewLine))
        {
            // get sha256 of the query, make a temp file with a windows-friendly filename derived from it
            // and persist the query. use the temp file as the query file input instead of a simple arg
            var hash = BitConverter.ToString(SHA256.HashData(Encoding.UTF8.GetBytes(normalized)));
            var queryFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"{hash}.jq");
            if (!File.Exists(queryFile))
                await File.WriteAllTextAsync(queryFile, normalized);

            var jq = await Cli.Wrap(jqpath)
                .WithArguments(["-r", "-f", queryFile])
                .WithStandardInputPipe(PipeSource.FromString(json, Encoding.UTF8))
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync(Encoding.UTF8);

            return jq.StandardOutput.Trim();
        }
        else
        {
            var jq = await Cli.Wrap(jqpath)
                .WithArguments(["-r", query])
                .WithStandardInputPipe(PipeSource.FromString(json, Encoding.UTF8))
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync(Encoding.UTF8);

            return jq.StandardOutput.Trim();
        }
    }
}
