# .NET 10 Native AOT CLI Template

A modern, mildly opinionated starter repository for building lightning-fast, standalone command-line tools with **C# and .NET 10 Native AOT**.

Engineered for sub-10ms startup times, cross-platform distribution, strict compile-time trimmer verification, and first-class integration with **AI Agents & LLM Tool Calling**.

---

## ⚡ Features at a Glance

- **🚀 .NET 10 Native AOT**: Zero-runtime JIT overhead, ~10MB standalone native executable, microsecond startup.
- **🛡️ Strict AOT & Trimmer Analyzers**: Preconfigured with `PublishAot`, `IsAotCompatible`, `EnableTrimAnalyzer`, `EnableAotAnalyzer`, and warnings escalated to errors (`IL2026`, `IL3050`, `CA1000`, etc.).
- **🤖 Agent & LLM Development Ready**:
  - Global `--json` mode emitting a standardized `CliResult<T>` envelope for seamless machine parsing.
  - Built-in `schema --format openai` exporting tool definitions for OpenAI, Claude, Gemini, and MCP runtimes.
  - Dedicated [AGENTS.md](AGENTS.md) guide, `.cursorrules`, and Copilot instructions.
- **🎨 Dual-Mode Output**:
  - **Interactive Human Mode**: Rich terminal styling, rounded borders, tables, and markup powered by [Spectre.Console](https://spectreconsole.net/).
  - **Agent / Script Mode**: Clean, deterministic JSON output on standard output with exit code tracking.
- **📦 Central Package Management (CPM)**: Single source of truth for NuGet dependencies in `Directory.Packages.props`.
- **🌐 Matrix Cross-Platform CI/CD**:
  - **GitHub Actions**: Automated test and native release packaging for Linux (`x64`, `arm64`), macOS (`osx-arm64`, `osx-x64`), and Windows (`win-x64`, `win-arm64`).
  - **GitLab CI**: Production-ready `.gitlab-ci.yml` pipeline with Alpine musl and glibc Native AOT matrix jobs.
- **📥 One-Line Installers**: POSIX `scripts/install.sh` and Windows PowerShell `scripts/install.ps1`.
- **🧪 Zero-Leak Test Suite**: xUnit test suite featuring `TestConsoleContext` for thread-safe CLI output assertions.

---

## 📁 Repository Structure

```
├── .editorconfig                # Strict C# & trimmer analyzer severity rules
├── .gitattributes               # Line-ending and binary file handling
├── .gitignore                   # Standard .NET + Native AOT artifacts
├── global.json                  # Pinned .NET 10 SDK with feature rollForward
├── Directory.Build.props        # Solution-wide build, AOT flags, and compiler warnings
├── Directory.Packages.props     # Central Package Management (CPM)
├── NativeAotCliTemplate.slnx    # Modern XML solution file
├── Makefile                     # Developer convenience shortcuts
├── AGENTS.md                    # Instructions for AI coding agents & LLMs
├── .cursorrules                 # AI coding rules for Cursor IDE
├── .gitlab-ci.yml               # GitLab CI matrix AOT build & release pipeline
├── .github/
│   ├── copilot-instructions.md  # Copilot Workspace rules
│   └── workflows/
│       ├── ci.yml               # Multi-OS build & test matrix
│       └── release.yml          # Native binary release packaging & SHA256
├── scripts/
│   ├── install.sh               # Unix installer (Linux & macOS, x64 & arm64)
│   ├── install.ps1              # Windows PowerShell installer
│   ├── build.sh                 # Local Unix build and AOT publish script
│   └── build.ps1                # Local Windows build and AOT publish script
├── src/
│   └── NativeAotCli/
│       ├── Program.cs           # ConsoleAppFramework command routing
│       ├── Commands/            # Vertical slice command implementations
│       │   ├── GreetCommand.cs  # Validation, options, and dual-mode rendering
│       │   ├── InfoCommand.cs   # System & Native AOT runtime diagnostics
│       │   └── SchemaCommand.cs # AI tool calling & JSON schema exporter
│       ├── Common/
│       │   ├── CliResult.cs     # Standardized agent response envelope
│       │   ├── ConsoleOutput.cs # Testable human/JSON output dispatcher
│       │   └── AppJsonContext.cs# System.Text.Json source generation context
│       └── Models/
│           └── Models.cs        # Domain models & OpenAI tool schema types
└── tests/
    └── NativeAotCli.Tests/
        ├── AssemblyInfo.cs      # Sequential test behavior
        ├── TestConsoleContext.cs# Output redirection test fixture
        ├── CommandTests.cs      # Unit tests for CLI commands
        └── JsonOutputTests.cs   # JSON serialization & schema verification
```

---

## 🚀 Quick Start

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or higher.
- C++ compiler toolchain for Native AOT:
  - **Linux**: `clang` and `zlib1g-dev` (`sudo apt-get install clang zlib1g-dev`)
  - **macOS**: Xcode Command Line Tools (`xcode-select --install`)
  - **Windows**: Visual Studio 2022 C++ build tools or Visual C++ Redistributable

### 1. Run in Development Mode (Managed IL / JIT)

```bash
# Display help
dotnet run --project src/NativeAotCli -- --help

# Run greet command
dotnet run --project src/NativeAotCli -- greet --name "Developer" --count 2

# Inspect runtime diagnostics
dotnet run --project src/NativeAotCli -- info
```

### 2. Run Tests

```bash
dotnet test
```

### 3. Compile Native AOT Standalone Binary

```bash
# Using Makefile
make aot

# Or using script directly
./scripts/build.sh

# Or with dotnet CLI
dotnet publish src/NativeAotCli/NativeAotCli.csproj -c Release -r osx-arm64 -o publish/osx-arm64
```

Verify execution:
```bash
./publish/osx-arm64/aotcli info
```

---

## 🤖 AI Agent & LLM Integration

This template is purpose-built to act as a set of executable tools for autonomous AI agents (Claude Code, OpenAI Assistants, AutoGen, CrewAI, Gemini, MCP servers).

### 1. Discover Available Tools (OpenAI Function Calling Format)

```bash
aotcli schema --format openai
```

**Output:**
```json
[
  {
    "type": "function",
    "function": {
      "name": "greet",
      "description": "Greet a user or entity with configurable formatting and repetition.",
      "parameters": {
        "type": "object",
        "properties": {
          "name": {
            "type": "string",
            "description": "Recipient name to greet (default: World)"
          },
          "count": {
            "type": "integer",
            "description": "Number of times to repeat greeting (1-100, default: 1)"
          },
          "shout": {
            "type": "boolean",
            "description": "Convert greeting to uppercase"
          },
          "json": {
            "type": "boolean",
            "description": "Output structured JSON for machine or agent consumption"
          }
        },
        "required": []
      }
    }
  }
]
```

### 2. Execute via Agent in Machine-Readable JSON Mode

Append `--json` (or `-j`) to any command:

```bash
aotcli greet --name "Agent 007" --shout --json
```

**Response:**
```json
{
  "success": true,
  "data": {
    "message": "HELLO, AGENT 007!",
    "recipient": "Agent 007",
    "count": 1,
    "timestampUtc": "2026-10-04T01:57:39.624967+00:00"
  },
  "exitCode": 0,
  "timestampUtc": "2026-10-04T01:57:39.642771+00:00"
}
```

If validation fails:
```json
{
  "success": false,
  "data": null,
  "error": "Argument 'count' must be between 1 and 100. Provided value: 0.",
  "exitCode": 1,
  "timestampUtc": "2026-10-04T01:57:39.642771+00:00"
}
```

---

## 🛠️ Adding a New Command

1. **Create the command method** in `src/NativeAotCli/Commands/`:
   ```csharp
   namespace NativeAotCli.Commands;

   public static class PingCommand
   {
       /// <summary>
       /// Ping a remote host.
       /// </summary>
       /// <param name="host">-h, Target hostname or IP</param>
       /// <param name="json">-j, Output structured JSON</param>
       public static int Execute(string host = "localhost", bool json = false)
       {
           var data = new PingData(host, "pong", 12);
           ConsoleOutput.Render(
               jsonMode: json,
               data: data,
               typeInfo: AppJsonContext.Default.CliResultPingData,
               renderHuman: (d, console) => console.MarkupLine($"[green]Pong from {d.Host} in {d.LatencyMs}ms[/]"));
           return 0;
       }
   }
   ```

2. **Register in `Program.cs`**:
   ```csharp
   app.Add("ping", PingCommand.Execute);
   ```

3. **Register in `AppJsonContext.cs`**:
   ```csharp
   [JsonSerializable(typeof(CliResult<PingData>))]
   [JsonSerializable(typeof(PingData))]
   ```

4. **Verify compile & test**:
   ```bash
   dotnet test
   ```

---

## 📦 Installation Scripts

### Unix (Linux & macOS)

Install via POSIX shell script to `~/.local/bin/aotcli`:
```bash
./scripts/install.sh
```

Or remotely from your release repository:
```bash
curl -fsSL https://raw.githubusercontent.com/username/native_aot_template/main/scripts/install.sh | bash
```

### Windows (PowerShell)

Install via PowerShell script to `$HOME\.local\bin\aotcli.exe`:
```powershell
.\scripts\install.ps1
```

Or remotely:
```powershell
irm https://raw.githubusercontent.com/username/native_aot_template/main/scripts/install.ps1 | iex
```

---

## 🌐 CI/CD Matrix Pipelines

### GitHub Actions
- **`.github/workflows/ci.yml`**: Runs multi-OS test matrix (`ubuntu-latest`, `macos-latest`, `windows-latest`) on pull requests and pushes to `main`.
- **`.github/workflows/release.yml`**: Triggers on tag pushes (`v*`), compiling standalone Native AOT binaries across:
  - `linux-x64` (`.tar.gz`)
  - `osx-arm64` (`.tar.gz`)
  - `osx-x64` (`.tar.gz`)
  - `win-x64` (`.zip`)
  - `win-arm64` (`.zip`)
  Includes automatic SHA-256 checksum generation and GitHub Release publication.

### GitLab CI
- **`.gitlab-ci.yml`**: Full matrix pipeline building `linux-x64`, `linux-musl-x64` (Alpine static), and `linux-arm64` Native AOT artifacts with GitLab Release asset linking.

---

## 📜 Developer Commands (`Makefile`)

| Command | Description |
| :--- | :--- |
| `make build` | Builds solution in Debug configuration |
| `make test` | Executes all xUnit unit & integration tests |
| `make aot` | Publishes optimized Native AOT binary for host OS |
| `make schema` | Dumps CLI schema in JSON format |
| `make schema-openai` | Dumps OpenAI / Agent function calling schema |
| `make install` | Installs local AOT binary to `~/.local/bin` |
| `make clean` | Removes build, publish, and test artifacts |

---

## 📄 License

This template is licensed under the [MIT License](LICENSE).
