using System.Text.Json;
using Cli.Commands;
using Cli.Common;
using Xunit;

namespace Integration;

public class SchemaIntegrationTests
{
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

    [Theory]
    [InlineData("json")]
    [InlineData("openai")]
    [InlineData("markdown")]
    public void SchemaCommand_InJsonMode_ReturnsEnvelope(string format)
    {
        using var ctx = new TestConsoleContext();
        var exitCode = SchemaCommand.Execute([], format, json: true);
        Assert.Equal(0, exitCode);

        using var doc = JsonDocument.Parse(ctx.Output);
        Assert.True(doc.RootElement.GetProperty("success").GetBoolean());
        Assert.True(doc.RootElement.TryGetProperty("data", out _));
    }

    [Fact]
    public void SchemaCommand_InvalidFormatInJsonMode_ReturnsStructuredError()
    {
        using var ctx = new TestConsoleContext();
        var exitCode = SchemaCommand.Execute([], "unknown_format", json: true);
        Assert.Equal(1, exitCode);

        var parsed = JsonSerializer.Deserialize(ctx.Output, AppJsonContext.Default.CliResultString);
        Assert.NotNull(parsed);
        Assert.False(parsed.Success);
        Assert.Contains("Unsupported format 'unknown_format'", parsed.Error, StringComparison.Ordinal);
    }
}
