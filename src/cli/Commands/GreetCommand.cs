using Cli.Common;
using Cli.Models;
using Spectre.Console;

namespace Cli.Commands;

/// <summary>
/// Handles the 'greet' command.
/// Demonstrates option parsing, validation, and multi-mode (human / JSON) output.
/// </summary>
public static class GreetCommand
{
    /// <summary>
    /// Greet a user or entity with configurable formatting and repetition.
    /// </summary>
    /// <param name="name">-n, Recipient name to greet (default: World)</param>
    /// <param name="count">-c, Number of times to repeat greeting (1-100, default: 1)</param>
    /// <param name="shout">-s, Convert greeting to uppercase</param>
    /// <param name="json">-j, Output structured JSON for machine or agent consumption</param>
    public static int Execute(
        string name = "World",
        int count = 1,
        bool shout = false,
        bool json = false)
    {
        if (count < 1 || count > 100)
        {
            return ConsoleOutput.RenderError(
                $"Argument 'count' must be between 1 and 100. Provided value: {count}.",
                jsonMode: json);
        }

        var greeting = shout ? $"HELLO, {name.ToUpperInvariant()}!" : $"Hello, {name}!";
        var data = new GreetingData(
            Message: greeting,
            Recipient: name,
            Count: count,
            TimestampUtc: DateTimeOffset.UtcNow);

        ConsoleOutput.Render(
            jsonMode: json,
            data: data,
            typeInfo: AppJsonContext.Default.CliResultGreetingData,
            renderHuman: (d, console) =>
            {
                var panel = new Panel(new Markup($"[bold cyan]{Markup.Escape(d.Message)}[/]"))
                {
                    Header = new PanelHeader($"[green]Native AOT Greeting (x{d.Count})[/]"),
                    Border = BoxBorder.Rounded
                };

                for (var i = 0; i < d.Count; i++)
                {
                    console.Write(panel);
                }
            });

        return 0;
    }
}
