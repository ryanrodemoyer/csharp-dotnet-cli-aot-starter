using System.Text.Json.Serialization;
using Cli.Models;
using ConsoleAppFramework;

namespace Cli.Common;

/// <summary>
/// Source-generated JSON serializer context for 100% Native AOT compatibility.
/// Zero runtime reflection, zero trimmer warnings, high-performance serialization.
/// </summary>
[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(CliResult<GreetingData>))]
[JsonSerializable(typeof(CliResult<SystemInfoData>))]
[JsonSerializable(typeof(CliResult<List<OpenAiTool>>))]
[JsonSerializable(typeof(CliResult<string>))]
[JsonSerializable(typeof(GreetingData))]
[JsonSerializable(typeof(SystemInfoData))]
[JsonSerializable(typeof(List<OpenAiTool>))]
[JsonSerializable(typeof(OpenAiTool))]
[JsonSerializable(typeof(OpenAiFunction))]
[JsonSerializable(typeof(OpenAiFunctionParameters))]
[JsonSerializable(typeof(OpenAiProperty))]
[JsonSerializable(typeof(Dictionary<string, OpenAiProperty>))]
[JsonSerializable(typeof(List<string>))]
[JsonSerializable(typeof(CommandHelpDefinition[]))]
[JsonSerializable(typeof(CommandHelpDefinition))]
[JsonSerializable(typeof(CommandOptionHelpDefinition[]))]
[JsonSerializable(typeof(CommandOptionHelpDefinition))]
[JsonSerializable(typeof(string[]))]
public sealed partial class AppJsonContext : JsonSerializerContext
{
}
