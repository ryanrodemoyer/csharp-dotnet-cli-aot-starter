using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Spectre.Console;

namespace NativeAotCli.Common;

/// <summary>
/// Output abstraction supporting rich human UI (via Spectre.Console) and clean JSON for AI agents.
/// Fully testable by substituting the underlying IAnsiConsole.
/// </summary>
public static class ConsoleOutput
{
    private static IAnsiConsole? _console;

    /// <summary>
    /// Gets or sets the active console instance (defaults to Spectre's AnsiConsole.Console).
    /// </summary>
    public static IAnsiConsole Console
    {
        get => _console ?? AnsiConsole.Console;
        set => _console = value;
    }

    /// <summary>
    /// Reset console to default standard output.
    /// </summary>
    public static void Reset() => _console = null;

    /// <summary>
    /// Render structured data either as JSON or through a human-friendly Spectre render callback.
    /// </summary>
    public static void Render<T>(
        bool jsonMode,
        T data,
        JsonTypeInfo<CliResult<T>> typeInfo,
        Action<T, IAnsiConsole> renderHuman)
    {
        if (jsonMode)
        {
            var result = CliResult.Ok(data);
            var json = JsonSerializer.Serialize(result, typeInfo);
            Console.Profile.Out.Writer.WriteLine(json);
        }
        else
        {
            renderHuman(data, Console);
        }
    }

    /// <summary>
    /// Render raw JSON directly to active output writer.
    /// </summary>
    public static void RenderRawJson<T>(T data, JsonTypeInfo<T> typeInfo)
    {
        var json = JsonSerializer.Serialize(data, typeInfo);
        Console.Profile.Out.Writer.WriteLine(json);
    }

    /// <summary>
    /// Write raw text line to active output writer.
    /// </summary>
    public static void WriteLine(string text = "")
    {
        Console.Profile.Out.Writer.WriteLine(text);
    }

    /// <summary>
    /// Render an error message to active console or as a structured JSON error.
    /// </summary>
    public static int RenderError(
        string message,
        bool jsonMode = false,
        int exitCode = 1)
    {
        if (jsonMode)
        {
            var result = CliResult.Fail<string>(message, exitCode);
            var json = JsonSerializer.Serialize(result, AppJsonContext.Default.CliResultString);
            Console.Profile.Out.Writer.WriteLine(json);
        }
        else
        {
            Console.MarkupLine($"[red bold]Error:[/] [red]{Markup.Escape(message)}[/]");
        }

        return exitCode;
    }
}
