using NativeAotCli.Commands;
using Xunit;

namespace NativeAotCli.Tests;

public class CommandTests
{
    [Fact]
    public void GreetCommand_WithDefaults_ReturnsSuccess()
    {
        using var ctx = new TestConsoleContext();
        var exitCode = GreetCommand.Execute();
        Assert.Equal(0, exitCode);
        Assert.Contains("Hello, World!", ctx.Output, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("Alice", 1, false, "Hello, Alice!")]
    [InlineData("Bob", 3, true, "HELLO, BOB!")]
    public void GreetCommand_WithCustomParameters_ReturnsSuccess(string name, int count, bool shout, string expected)
    {
        using var ctx = new TestConsoleContext();
        var exitCode = GreetCommand.Execute(name: name, count: count, shout: shout);
        Assert.Equal(0, exitCode);
        Assert.Contains(expected, ctx.Output, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(101)]
    public void GreetCommand_WithInvalidCount_ReturnsFailure(int invalidCount)
    {
        using var ctx = new TestConsoleContext();
        var exitCode = GreetCommand.Execute(count: invalidCount);
        Assert.Equal(1, exitCode);
        Assert.Contains("Argument 'count' must be between 1 and 100", ctx.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void InfoCommand_ExecutesSuccessfully()
    {
        using var ctx = new TestConsoleContext();
        var exitCode = InfoCommand.Execute();
        Assert.Equal(0, exitCode);
        Assert.Contains("Diagnostics", ctx.Output, StringComparison.Ordinal);
    }
}
