using System.Text.Json;
using NativeAotCli.Commands;
using NativeAotCli.Common;
using NativeAotCli.Models;
using Xunit;

namespace NativeAotCli.Tests;

public class JsonOutputTests
{
    [Fact]
    public void GreetCommand_InJsonMode_ReturnsValidStructuredJson()
    {
        using var ctx = new TestConsoleContext();
        var exitCode = GreetCommand.Execute(name: "TestAgent", count: 2, shout: true, json: true);
        Assert.Equal(0, exitCode);

        var output = ctx.Output.Trim();
        var parsed = JsonSerializer.Deserialize(output, AppJsonContext.Default.CliResultGreetingData);

        Assert.NotNull(parsed);
        Assert.True(parsed.Success);
        Assert.NotNull(parsed.Data);
        Assert.Equal("HELLO, TESTAGENT!", parsed.Data.Message);
        Assert.Equal("TestAgent", parsed.Data.Recipient);
        Assert.Equal(2, parsed.Data.Count);
    }

    [Fact]
    public void GreetCommand_InvalidCountInJsonMode_ReturnsStructuredError()
    {
        using var ctx = new TestConsoleContext();
        var exitCode = GreetCommand.Execute(count: 0, json: true);
        Assert.Equal(1, exitCode);

        var output = ctx.Output.Trim();
        var parsed = JsonSerializer.Deserialize(output, AppJsonContext.Default.CliResultString);

        Assert.NotNull(parsed);
        Assert.False(parsed.Success);
        Assert.Equal(1, parsed.ExitCode);
        Assert.Contains("Argument 'count' must be between 1 and 100", parsed.Error, StringComparison.Ordinal);
    }

    [Fact]
    public void InfoCommand_InJsonMode_ReturnsSystemDiagnostics()
    {
        using var ctx = new TestConsoleContext();
        var exitCode = InfoCommand.Execute(json: true);
        Assert.Equal(0, exitCode);

        var output = ctx.Output.Trim();
        var parsed = JsonSerializer.Deserialize(output, AppJsonContext.Default.CliResultSystemInfoData);

        Assert.NotNull(parsed);
        Assert.True(parsed.Success);
        Assert.NotNull(parsed.Data);
        Assert.NotEmpty(parsed.Data.ApplicationName);
        Assert.NotEmpty(parsed.Data.DotNetVersion);
        Assert.NotEmpty(parsed.Data.OsDescription);
        Assert.True(parsed.Data.ProcessorCount > 0);
    }

    [Theory]
    [InlineData("json")]
    [InlineData("openai")]
    [InlineData("markdown")]
    public void SchemaCommand_SupportedFormats_ExecuteSuccessfully(string format)
    {
        using var ctx = new TestConsoleContext();
        var exitCode = SchemaCommand.Execute([], format);
        Assert.Equal(0, exitCode);
        Assert.NotEmpty(ctx.Output);
    }

    [Fact]
    public void SchemaCommand_InvalidFormat_ReturnsFailure()
    {
        using var ctx = new TestConsoleContext();
        var exitCode = SchemaCommand.Execute([], "unknown_format");
        Assert.Equal(1, exitCode);
        Assert.Contains("Unsupported format 'unknown_format'", ctx.Output, StringComparison.Ordinal);
    }
}
