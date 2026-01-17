using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.Buffered;

namespace Devlooped;

/// <summary>
/// Represents the result of a JQ query execution.
/// </summary>
public class JqResult
{
    /// <summary>
    /// Gets the exit code from the JQ process.
    /// </summary>
    public int ExitCode { get; }

    /// <summary>
    /// Gets the standard output from the JQ process.
    /// </summary>
    public string StandardOutput { get; }

    /// <summary>
    /// Gets the standard error from the JQ process.
    /// </summary>
    public string StandardError { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="JqResult"/> class.
    /// </summary>
    public JqResult(int exitCode, string standardOutput, string standardError)
    {
        ExitCode = exitCode;
        StandardOutput = standardOutput;
        StandardError = standardError;
    }

    /// <summary>
    /// Implicitly converts a <see cref="JqResult"/> to a <see cref="string"/> by returning the <see cref="StandardOutput"/>.
    /// </summary>
    public static implicit operator string(JqResult result) => result?.StandardOutput ?? string.Empty;
}

/// <summary>
/// Parameters for executing a JQ query.
/// </summary>
public class JqParams
{
    /// <summary>
    /// Gets or sets the JSON input to process.
    /// </summary>
    public string? Json { get; set; }

    /// <summary>
    /// Gets the JQ query/filter to execute.
    /// </summary>
    public string Query { get; }

    /// <summary>
    /// Gets or sets a value indicating whether to output raw strings, not JSON texts (-r, --raw-output).
    /// </summary>
    public bool RawOutput { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to produce compact instead of pretty-printed output (-c, --compact-output).
    /// </summary>
    public bool? CompactOutput { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to disable colored output (-M, --monochrome-output).
    /// </summary>
    public bool? MonochromeOutput { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to enable colored output (-C, --color-output).
    /// </summary>
    public bool? ColorOutput { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to read the entire input stream into a large array and run the filter just once (-s, --slurp).
    /// </summary>
    public bool? Slurp { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to not read any input; filter is run with null input (-n, --null-input).
    /// </summary>
    public bool? NullInput { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to set the exit status code based on the output of the program (-e, --exit-status).
    /// </summary>
    public bool? ExitStatus { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to output valid JSON strings with ASCII characters escaped (-a, --ascii-output).
    /// </summary>
    public bool? AsciiOutput { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to sort object keys in output (-S, --sort-keys).
    /// </summary>
    public bool? SortKeys { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to not output newlines after each JSON object (-j, --join-output).
    /// </summary>
    public bool? JoinOutput { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to use tabs for indentation (--tab).
    /// </summary>
    public bool? Tab { get; set; }

    /// <summary>
    /// Gets or sets the indent level for pretty-printing (--indent n).
    /// </summary>
    public int? Indent { get; set; }

    /// <summary>
    /// Gets or sets string variables to pass to the JQ program (--arg name value).
    /// </summary>
    public Dictionary<string, string>? Args { get; set; }

    /// <summary>
    /// Gets or sets JSON variables to pass to the JQ program (--argjson name value).
    /// </summary>
    public Dictionary<string, string>? ArgsJson { get; set; }

    /// <summary>
    /// Gets or sets files to read as JSON arrays and bind to variables (--slurpfile name filename).
    /// </summary>
    public Dictionary<string, string>? SlurpFiles { get; set; }

    /// <summary>
    /// Gets or sets files to read as raw strings and bind to variables (--rawfile name filename).
    /// </summary>
    public Dictionary<string, string>? RawFiles { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="JqParams"/> class.
    /// </summary>
    /// <param name="query">The JQ query/filter to execute. Defaults to "." if not specified.</param>
    public JqParams(string? query = ".")
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new ArgumentException("Query cannot be null or empty.", nameof(query));
        Query = query!; // null-forgiving operator since we validated above
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JqParams"/> class with JSON and query.
    /// </summary>
    /// <param name="json">The JSON input to process.</param>
    /// <param name="query">The JQ query/filter to execute.</param>
    public JqParams(string json, string query) : this(query)
    {
        Json = json;
    }
}

/// <summary>
/// Executes JQ queries on JSON input.
/// </summary>
/// <remarks>
/// Learn more about JQ at https://jqlang.github.io/jq/.
/// </remarks>
static partial class JQ
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
        if (!OperatingSystem.IsWindows())
        {
#pragma warning disable CA1416 // Validate platform compatibility
            try
            {
                Process.Start(jqpath, "--version").WaitForExit();
            }
            catch (Win32Exception)
            {
                try
                {
                    File.SetUnixFileMode(jqpath, UnixFileMode.UserExecute);
                    Process.Start(jqpath, "--version").WaitForExit();
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
#pragma warning restore CA1416 // Validate platform compatibility
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
        var result = await ExecuteAsync(new JqParams(json, query));
        return result.StandardOutput;
    }

    /// <summary>
    /// Execute the query with the specified parameters and return the result.
    /// </summary>
    public static async Task<JqResult> ExecuteAsync(JqParams parameters)
    {
        if (parameters == null)
            throw new ArgumentNullException(nameof(parameters));

        if (!File.Exists(jqpath))
            throw new FileNotFoundException($"JQ executable not found.", jqpath);

        // Validate that either JSON input is provided or NullInput is true
        if (parameters.Json == null && parameters.NullInput != true)
            throw new ArgumentException("Either Json must be provided or NullInput must be true.", nameof(parameters));

        var normalized = parameters.Query.ReplaceLineEndings().Trim();
        var args = new List<string>();
        string? queryFile = null;

        // Build arguments based on parameters
        if (parameters.RawOutput)
            args.Add("-r");

        if (parameters.CompactOutput == true)
            args.Add("-c");

        if (parameters.MonochromeOutput == true)
            args.Add("-M");

        if (parameters.ColorOutput == true)
            args.Add("-C");

        if (parameters.Slurp == true)
            args.Add("-s");

        if (parameters.NullInput == true)
            args.Add("-n");

        if (parameters.ExitStatus == true)
            args.Add("-e");

        if (parameters.AsciiOutput == true)
            args.Add("-a");

        if (parameters.SortKeys == true)
            args.Add("-S");

        if (parameters.JoinOutput == true)
            args.Add("-j");

        if (parameters.Tab == true)
            args.Add("--tab");

        if (parameters.Indent.HasValue)
        {
            args.Add("--indent");
            args.Add(parameters.Indent.Value.ToString());
        }

        // Add --arg parameters
        if (parameters.Args != null)
        {
            foreach (var arg in parameters.Args)
            {
                args.Add("--arg");
                args.Add(arg.Key);
                args.Add(arg.Value);
            }
        }

        // Add --argjson parameters
        if (parameters.ArgsJson != null)
        {
            foreach (var arg in parameters.ArgsJson)
            {
                args.Add("--argjson");
                args.Add(arg.Key);
                args.Add(arg.Value);
            }
        }

        // Add --slurpfile parameters
        if (parameters.SlurpFiles != null)
        {
            foreach (var file in parameters.SlurpFiles)
            {
                // Validate file exists to prevent path traversal and access to sensitive files
                if (!File.Exists(file.Value))
                    throw new FileNotFoundException($"Slurp file not found: {file.Value}", file.Value);

                args.Add("--slurpfile");
                args.Add(file.Key);
                args.Add(file.Value);
            }
        }

        // Add --rawfile parameters
        if (parameters.RawFiles != null)
        {
            foreach (var file in parameters.RawFiles)
            {
                // Validate file exists to prevent path traversal and access to sensitive files
                if (!File.Exists(file.Value))
                    throw new FileNotFoundException($"Raw file not found: {file.Value}", file.Value);

                args.Add("--rawfile");
                args.Add(file.Key);
                args.Add(file.Value);
            }
        }

        // Handle multi-line queries
        if (normalized.Contains(Environment.NewLine))
        {
            // get sha256 of the query, make a temp file with a windows-friendly filename derived from it
            // and persist the query. use the temp file as the query file input instead of a simple arg
            var hash = HashString(normalized);
            queryFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"{hash}.jq");
            if (!File.Exists(queryFile))
            {
                lock (syncLock)
                {
                    if (!File.Exists(queryFile))
                        File.WriteAllText(queryFile, normalized);
                }
            }

            args.Add("-f");
            args.Add(queryFile);
        }
        else
        {
            args.Add(normalized);
        }

        var command = Cli.Wrap(jqpath)
            .WithArguments(args)
            .WithValidation(CommandResultValidation.None);

        // Add input if provided
        if (parameters.Json != null)
            command = command.WithStandardInputPipe(PipeSource.FromString(parameters.Json, Encoding.UTF8));

        var jq = await command.ExecuteBufferedAsync(Encoding.UTF8);

        return new JqResult(jq.ExitCode, jq.StandardOutput.Trim(), jq.StandardError.Trim());
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
