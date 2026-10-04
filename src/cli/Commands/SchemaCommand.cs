using System.Globalization;
using System.Text;
using Cli.Common;
using Cli.Models;
using ConsoleAppFramework;

namespace Cli.Commands;

/// <summary>
/// Handles the 'schema' command.
/// Exports machine-readable schemas and OpenAI/Agent tool calling definitions.
/// </summary>
public static class SchemaCommand
{
    private static Func<CommandHelpDefinition[]>? _schemaProvider;

    /// <summary>
    /// Initialize the schema provider delegate from the application builder.
    /// </summary>
    public static void Initialize(Func<CommandHelpDefinition[]> schemaProvider)
    {
        _schemaProvider = schemaProvider;
    }

    /// <summary>
    /// Export CLI command schema for AI agents, tool calling, or documentation.
    /// </summary>
    /// <param name="format">-f, Output schema format: json, openai, markdown (default: json)</param>
    /// <param name="json">-j, Wrap output and errors in the standard JSON result envelope</param>
    public static int Run(string format = "json", bool json = false)
    {
        var schema = _schemaProvider?.Invoke() ?? [];
        return Execute(schema, format, json);
    }

    /// <summary>
    /// Export CLI command schema for AI agents, tool calling, or documentation.
    /// </summary>
    /// <param name="schema">Provided automatically by the app.</param>
    /// <param name="format">Output schema format: json, openai, markdown (default: json)</param>
    /// <param name="json">Wrap output and errors in the standard JSON result envelope</param>
    public static int Execute(
        CommandHelpDefinition[] schema,
        string format = "json",
        bool json = false)
    {
        return format.ToLowerInvariant() switch
        {
            "openai" => RenderOpenAiTools(schema, json),
            "markdown" or "md" => RenderMarkdown(schema, json),
            "json" => RenderJson(schema, json),
            _ => ConsoleOutput.RenderError(
                $"Unsupported format '{format}'. Valid options: json, openai, markdown.",
                jsonMode: json)
        };
    }

    private static int RenderJson(CommandHelpDefinition[] schema, bool json)
    {
        ConsoleOutput.Render(
            jsonMode: json,
            data: schema,
            typeInfo: AppJsonContext.Default.CliResultCommandHelpDefinitionArray,
            renderHuman: (d, _) => ConsoleOutput.RenderRawJson(d, AppJsonContext.Default.CommandHelpDefinitionArray));
        return 0;
    }

    private static int RenderOpenAiTools(CommandHelpDefinition[] schema, bool json)
    {
        var tools = new List<OpenAiTool>();

        foreach (var cmd in schema)
        {
            var properties = new Dictionary<string, OpenAiProperty>();
            var required = new List<string>();

            foreach (var opt in cmd.Options)
            {
                var optName = opt.Options.FirstOrDefault(o => o.StartsWith("--", StringComparison.Ordinal))?.TrimStart('-')
                              ?? opt.Options.FirstOrDefault()?.TrimStart('-')
                              ?? "arg";

                var jsonType = MapToJsonType(opt.ValueTypeName);
                properties[optName] = new OpenAiProperty(
                    Type: jsonType,
                    Description: opt.Description);

                if (opt.IsRequired && !opt.IsFlag)
                {
                    required.Add(optName);
                }
            }

            tools.Add(new OpenAiTool(
                Type: "function",
                Function: new OpenAiFunction(
                    Name: cmd.CommandName.Length == 0 ? "root" : cmd.CommandName.Replace(' ', '_'),
                    Description: cmd.Description,
                    Parameters: new OpenAiFunctionParameters(
                        Type: "object",
                        Properties: properties,
                        Required: required))));
        }

        ConsoleOutput.Render(
            jsonMode: json,
            data: tools,
            typeInfo: AppJsonContext.Default.CliResultListOpenAiTool,
            renderHuman: (d, _) => ConsoleOutput.RenderRawJson(d, AppJsonContext.Default.ListOpenAiTool));
        return 0;
    }

    private static int RenderMarkdown(CommandHelpDefinition[] schema, bool json)
    {
        var markdown = new StringBuilder();
        markdown.AppendLine("# CLI Command Reference").AppendLine();

        foreach (var cmd in schema)
        {
            var name = string.IsNullOrWhiteSpace(cmd.CommandName) ? "Default Command" : cmd.CommandName;
            markdown.AppendLine(CultureInfo.InvariantCulture, $"## `{name}`").AppendLine();
            if (!string.IsNullOrWhiteSpace(cmd.Description))
            {
                markdown.AppendLine(cmd.Description).AppendLine();
            }

            if (cmd.Options.Length > 0)
            {
                markdown.AppendLine("| Option | Type | Required | Description | Default |");
                markdown.AppendLine("| :--- | :--- | :--- | :--- | :--- |");
                foreach (var opt in cmd.Options)
                {
                    var flags = string.Join(", ", opt.Options.Select(o => $"`{o}`"));
                    var req = opt.IsRequired ? "**Yes**" : "No";
                    var defVal = string.IsNullOrEmpty(opt.DefaultValue) ? "-" : $"`{opt.DefaultValue}`";
                    markdown.AppendLine(CultureInfo.InvariantCulture, $"| {flags} | `{opt.ValueTypeName}` | {req} | {opt.Description} | {defVal} |");
                }

                markdown.AppendLine();
            }
        }

        ConsoleOutput.Render(
            jsonMode: json,
            data: markdown.ToString(),
            typeInfo: AppJsonContext.Default.CliResultString,
            renderHuman: (d, console) => console.Profile.Out.Writer.Write(d));
        return 0;
    }

    private static string MapToJsonType(string clrTypeName) => clrTypeName.ToLowerInvariant() switch
    {
        "int" or "int32" or "int64" or "long" => "integer",
        "bool" or "boolean" => "boolean",
        "double" or "float" or "decimal" => "number",
        "string[]" or "list`1" => "array",
        _ => "string"
    };
}
