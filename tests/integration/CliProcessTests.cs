using System.Diagnostics;
using System.Text.Json;
using Cli.Common;
using Xunit;

namespace Integration;

/// <summary>
/// Runs the real CLI entry point as a child process to verify the --json and exit code contract,
/// including errors raised by ConsoleAppFramework before any command method runs.
/// </summary>
public class CliProcessTests
{
    [Theory]
    [InlineData("greet", "--count", "abc", "--json")]   // unparseable value
    [InlineData("greet", "--bogus", "--json")]          // unknown flag
    [InlineData("nope", "--json")]                      // unknown command
    [InlineData("greet", "--count", "0", "-j")]         // command validation
    [InlineData("schema", "--format", "bad", "--json")] // command validation
    public void Errors_InJsonMode_AreEnvelopesOnStdout(params string[] args)
    {
        var result = RunCli(args);

        Assert.Equal(1, result.ExitCode);
        Assert.Empty(result.StdErr);
        var parsed = JsonSerializer.Deserialize(result.StdOut, AppJsonContext.Default.CliResultString);
        Assert.NotNull(parsed);
        Assert.False(parsed.Success);
        Assert.Equal(1, parsed.ExitCode);
        Assert.False(string.IsNullOrWhiteSpace(parsed.Error));
    }

    [Fact]
    public void UnknownCommand_InHumanMode_ExitsNonZero()
    {
        var result = RunCli("nope");

        Assert.Equal(1, result.ExitCode);
        Assert.Contains("Unknown command 'nope'", result.StdOut, StringComparison.Ordinal);
    }

    [Fact]
    public void Greet_InJsonMode_ReturnsSuccessEnvelope()
    {
        var result = RunCli("greet", "--name", "Agent", "--json");

        Assert.Equal(0, result.ExitCode);
        var parsed = JsonSerializer.Deserialize(result.StdOut, AppJsonContext.Default.CliResultGreetingData);
        Assert.NotNull(parsed);
        Assert.True(parsed.Success);
        Assert.Equal("Hello, Agent!", parsed.Data?.Message);
    }

    [Fact]
    public void Help_StillExitsZero()
    {
        var result = RunCli("--help");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Usage:", result.StdOut, StringComparison.Ordinal);
    }

    private static CliRun RunCli(params string[] args)
    {
        var startInfo = new ProcessStartInfo(Environment.GetEnvironmentVariable("DOTNET_HOST_PATH") ?? "dotnet")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            UseShellExecute = false
        };
        startInfo.ArgumentList.Add(Path.Combine(AppContext.BaseDirectory, "cli.dll"));
        foreach (var arg in args)
        {
            startInfo.ArgumentList.Add(arg);
        }

        using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start cli.");
        process.StandardInput.Close();
        var stdOut = process.StandardOutput.ReadToEndAsync();
        var stdErr = process.StandardError.ReadToEndAsync();
        if (!process.WaitForExit(TimeSpan.FromSeconds(30)))
        {
            process.Kill(entireProcessTree: true);
            throw new TimeoutException($"cli {string.Join(' ', args)} did not exit within 30 seconds.");
        }

        return new CliRun(process.ExitCode, stdOut.Result, stdErr.Result);
    }

    private sealed record CliRun(int ExitCode, string StdOut, string StdErr);
}
