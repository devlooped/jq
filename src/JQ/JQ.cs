using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
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
public static partial class JQ
{
    static readonly object syncLock = new();
    static readonly string jqpath;

    static JQ()
    {
        jqpath =
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && RuntimeInformation.ProcessArchitecture == Architecture.X64 ?
            System.IO.Path.Combine(AppContext.BaseDirectory, "lib", "jq-linux-amd64") :
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ?
            System.IO.Path.Combine(AppContext.BaseDirectory, "lib", "jq-linux-arm64") :
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && RuntimeInformation.ProcessArchitecture == Architecture.X86 ?
            System.IO.Path.Combine(AppContext.BaseDirectory, "lib", "jq-linux-i386") :
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && RuntimeInformation.ProcessArchitecture == Architecture.X64 ?
            System.IO.Path.Combine(AppContext.BaseDirectory, "lib", "jq-windows-amd64.exe") :
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && RuntimeInformation.ProcessArchitecture == Architecture.X86 ?
            System.IO.Path.Combine(AppContext.BaseDirectory, "lib", "jq-windows-i386.exe") :
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && RuntimeInformation.ProcessArchitecture == Architecture.X64 ?
            System.IO.Path.Combine(AppContext.BaseDirectory, "lib", "jq-macos-amd64") :
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ?
            System.IO.Path.Combine(AppContext.BaseDirectory, "lib", "jq-macos-arm64") :
            throw new PlatformNotSupportedException("Unsupported platform or architecture.");

        Debug.Assert(File.Exists(jqpath));

#if NET8_0_OR_GREATER
        if (!OperatingSystem.IsWindows() && !File.GetUnixFileMode(jqpath).HasFlag(UnixFileMode.UserExecute))
        {
            try
            {
                File.SetUnixFileMode(jqpath, UnixFileMode.UserExecute);
            }
            catch (UnauthorizedAccessException)
            {
                // In hosted environments, we might not be able to set the file mode.
                // So try using the temp directory instead by first copying the file there.
                var tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "jq");
                File.Copy(jqpath, tempPath, overwrite: true);
                jqpath = tempPath;
                File.SetUnixFileMode(jqpath, UnixFileMode.UserExecute);
            }
        }
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
            var hash = HashString(normalized);
            var queryFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"{hash}.jq");
            if (!File.Exists(queryFile))
            {
                lock (syncLock)
                {
                    if (!File.Exists(queryFile))
                        File.WriteAllText(queryFile, normalized);
                }
            }

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

    static string HashString(string input)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    static string ReplaceLineEndings(this string text)
#if NET8_0_OR_GREATER
        => LineEndingsExpr().Replace(text, Environment.NewLine);
#else
        => Regex.Replace(text, @"\r\n|\n|\r", Environment.NewLine);
#endif

#if NET8_0_OR_GREATER
    [GeneratedRegex(@"\r\n|\n|\r")]
    private static partial Regex LineEndingsExpr();
#endif
}
