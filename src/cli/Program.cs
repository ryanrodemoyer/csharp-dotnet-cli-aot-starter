using Cli.Commands;
using Cli.Common;
using ConsoleAppFramework;

var app = ConsoleApp.Create();

app.Add("greet", GreetCommand.Execute);
app.Add("info", InfoCommand.Execute);

SchemaCommand.Initialize(() => app.GetCliSchema());
app.Add("schema", SchemaCommand.Run);

// Framework errors (unparseable values, unknown flags, unhandled exceptions) are plain text by
// default. In --json mode, emit them as the same CliResult envelope that command errors use.
if (CliArguments.IsJsonMode(args))
{
    ConsoleApp.LogError = message => ConsoleOutput.RenderError(message, jsonMode: true);
}

// The framework prints help and exits 0 for an unknown command; report it as an error instead.
var unknownCommand = CliArguments.FindUnknownCommand(args, app.GetCliSchema().Select(c => c.CommandName));
if (unknownCommand is not null)
{
    ConsoleApp.LogError($"Unknown command '{unknownCommand}'. Run with --help to list available commands.");
    Environment.ExitCode = 1;
}
else
{
    app.Run(args);
}
