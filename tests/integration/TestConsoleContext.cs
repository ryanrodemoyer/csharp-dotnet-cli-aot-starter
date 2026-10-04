using Cli.Common;
using Spectre.Console;

namespace Integration;

/// <summary>
/// Scoped test fixture that redirects ConsoleOutput to an in-memory StringWriter.
/// </summary>
public sealed class TestConsoleContext : IDisposable
{
    private readonly StringWriter _writer = new();

    public TestConsoleContext()
    {
        ConsoleOutput.Console = AnsiConsole.Create(new AnsiConsoleSettings
        {
            Out = new AnsiConsoleOutput(_writer),
            Ansi = AnsiSupport.No
        });
    }

    public string Output => _writer.ToString();

    public void Dispose()
    {
        ConsoleOutput.Reset();
        _writer.Dispose();
    }
}
