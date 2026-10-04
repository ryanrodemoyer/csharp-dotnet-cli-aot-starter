using ConsoleAppFramework;
using NativeAotCli.Commands;

var app = ConsoleApp.Create();

app.Add("greet", GreetCommand.Execute);
app.Add("info", InfoCommand.Execute);

SchemaCommand.Initialize(() => app.GetCliSchema());
app.Add("schema", SchemaCommand.Run);

app.Run(args);
