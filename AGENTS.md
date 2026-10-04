# Agent Guidelines: Native AOT CLI Template

This document provides instructions and architectural rules for AI coding agents, autonomous agents, and LLMs interacting with or modifying this repository.

---

## 1. Project Overview & Architecture

- **Framework**: .NET 10 (`net10.0`)
- **Compilation Target**: Native AOT (Ahead-Of-Time compilation to native machine code)
- **CLI Engine**: `ConsoleAppFramework` (v5.x) — source-generated, zero reflection, microsecond startup
- **UI & Output**: `Spectre.Console` for interactive human output; `System.Text.Json` source generation for `--json` machine/agent output
- **Package Management**: Central Package Management (CPM) via `Directory.Packages.props`

---

## 2. Strict Native AOT Rules (Crucial for Agents)

Native AOT compiles C# directly into native machine code. JIT is disabled. Dynamic code generation and unchecked reflection will crash at runtime.

### 🚫 Prohibited Patterns:
1. **Reflection-based JSON Serialization**:
   - ❌ Never use `JsonSerializer.Serialize(obj)` or `JsonSerializer.Deserialize<T>(json)` without passing a `JsonTypeInfo<T>` from `AppJsonContext.Default`.
   - ❌ Never use `Newtonsoft.Json` (Json.NET) or reflection-heavy serializers.
2. **Dynamic Code & Reflection Emit**:
   - ❌ Do not use `System.Reflection.Emit`, `Activator.CreateInstance(type)` without trimmer annotations, or `Type.GetType(string)`.
3. **Unbounded Generics on Runtime Types**:
   - ❌ Do not invoke generic methods via reflection where type arguments cannot be statically analyzed.
4. **Interactive Stdin Prompts in Agent Execution**:
   - ❌ Commands must not hang on `Console.ReadLine()` or blocking interactive prompts when `--json` or non-interactive mode is requested.

### ✅ Required Patterns:
1. **Source Generated JSON**:
   - Whenever you create a new model or result DTO, register it in `src/cli/Common/AppJsonContext.cs`:
     ```csharp
     [JsonSerializable(typeof(CliResult<MyNewModel>))]
     [JsonSerializable(typeof(MyNewModel))]
     ```
2. **Dual-Mode Output**:
   - Every command should support both human-friendly output (via Spectre.Console) and structured JSON (via `ConsoleOutput.Render`):
     ```csharp
     ConsoleOutput.Render(
         jsonMode: json,
         data: myData,
         typeInfo: AppJsonContext.Default.CliResultMyData,
         renderHuman: (d, console) => {
             console.MarkupLine($"[green]{d.Message}[/]");
         });
     ```
3. **Centralized Packages**:
   - If adding a NuGet package, add its version to `Directory.Packages.props`, and reference it in `.csproj` without a `Version` attribute.

---

## 3. How to Add a New Command

1. **Create the Command class** in `src/cli/Commands/`:
   ```csharp
   namespace Cli.Commands;

   public static class MyFeatureCommand
   {
       /// <summary>
       /// Description for the command.
       /// </summary>
       /// <param name="paramName">-p, Description for parameter</param>
       /// <param name="json">-j, Output structured JSON</param>
       public static int Execute(string paramName, bool json = false)
       {
           // 1. Validate inputs
           if (string.IsNullOrWhiteSpace(paramName))
           {
               return ConsoleOutput.RenderError("Parameter is required.", jsonMode: json);
           }

           // 2. Perform business logic
           var result = new MyFeatureData(...);

           // 3. Render output
           ConsoleOutput.Render(
               jsonMode: json,
               data: result,
               typeInfo: AppJsonContext.Default.CliResultMyFeatureData,
               renderHuman: (d, console) => {
                   console.MarkupLine($"Result: {d.Value}");
               });

           return 0;
       }
   }
   ```

2. **Register in `Program.cs`**:
   ```csharp
   app.Add("my-feature", MyFeatureCommand.Execute);
   ```

3. **Register in `AppJsonContext.cs`**:
   ```csharp
   [JsonSerializable(typeof(CliResult<MyFeatureData>))]
   [JsonSerializable(typeof(MyFeatureData))]
   ```

4. **Add Unit Tests** in `tests/unit/`:
   - Use `using var ctx = new TestConsoleContext();` to safely assert both human markup and JSON outputs without touching global process state.

---

## 4. Invoking This CLI as an Agent Tool

Agents can discover available commands dynamically and invoke them as structured tools:

### Step 1: Discover Tools
Run:
```bash
cli schema --format openai
```
This prints the complete OpenAI function-calling schema for all commands in the CLI.

### Step 2: Call Commands Programmatically
Always pass `--json` (or `-j`) when running commands from an agent:
```bash
cli greet --name "Agent" --json
```

Output is guaranteed to be valid JSON matching the envelope:
```json
{
  "success": true,
  "data": { ... },
  "error": null,
  "exitCode": 0,
  "timestampUtc": "2026-10-04T00:00:00Z"
}
```

On error:
```json
{
  "success": false,
  "data": null,
  "error": "Explanation of what went wrong",
  "exitCode": 1,
  "timestampUtc": "2026-10-04T00:00:00Z"
}
```

---

## 5. Build, Test, and Verification Commands

- **Build**: `dotnet build starter.slnx -c Debug`
- **Test**: `dotnet test starter.slnx -c Debug`
- **Native AOT Publish**: `dotnet publish src/cli/cli.csproj -c Release -r <RID>`
- **Export Schema**: `dotnet run --project src/cli -- schema --format openai`
