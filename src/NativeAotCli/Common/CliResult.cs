namespace NativeAotCli.Common;

/// <summary>
/// Static factory methods for creating standardized CLI results.
/// </summary>
public static class CliResult
{
    public static CliResult<T> Ok<T>(T data) => new(
        Success: true,
        Data: data,
        Error: null,
        ExitCode: 0,
        TimestampUtc: DateTimeOffset.UtcNow);

    public static CliResult<T> Fail<T>(string error, int exitCode = 1) => new(
        Success: false,
        Data: default,
        Error: error,
        ExitCode: exitCode,
        TimestampUtc: DateTimeOffset.UtcNow);
}

/// <summary>
/// Uniform envelope for machine-readable JSON output and Agent tool responses.
/// </summary>
/// <typeparam name="T">Type of payload data.</typeparam>
public sealed record CliResult<T>(
    bool Success,
    T? Data,
    string? Error = null,
    int ExitCode = 0,
    DateTimeOffset TimestampUtc = default);
