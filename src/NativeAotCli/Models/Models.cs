namespace NativeAotCli.Models;

/// <summary>
/// Structured result for greet command.
/// </summary>
public sealed record GreetingData(
    string Message,
    string Recipient,
    int Count,
    DateTimeOffset TimestampUtc);

/// <summary>
/// Diagnostics and Native AOT runtime information.
/// </summary>
public sealed record SystemInfoData(
    string ApplicationName,
    string ApplicationVersion,
    string DotNetVersion,
    bool IsNativeAot,
    bool IsDynamicCodeSupported,
    string OsDescription,
    string OsArchitecture,
    string ProcessArchitecture,
    int ProcessorCount,
    long WorkingSetBytes,
    string FormattedWorkingSet,
    bool IsServerGc);

/// <summary>
/// OpenAI / LLM Tool Calling definition schema.
/// </summary>
public sealed record OpenAiTool(
    string Type,
    OpenAiFunction Function);

public sealed record OpenAiFunction(
    string Name,
    string Description,
    OpenAiFunctionParameters Parameters);

public sealed record OpenAiFunctionParameters(
    string Type,
    Dictionary<string, OpenAiProperty> Properties,
    List<string> Required);

public sealed record OpenAiProperty(
    string Type,
    string Description);
