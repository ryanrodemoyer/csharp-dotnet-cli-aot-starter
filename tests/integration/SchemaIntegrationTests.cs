using Cli.Commands;
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
}
