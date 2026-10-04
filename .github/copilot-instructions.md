# GitHub Copilot Instructions: .NET 10 Native AOT CLI Template

## Architecture & Code Standards
1. **Target**: .NET 10 (`net10.0`) Native AOT.
2. **Trimming & AOT Compatibility**:
   - Dynamic reflection and runtime code generation are strictly prohibited.
   - Any model serialized to or from JSON MUST be registered with `[JsonSerializable]` in `src/cli/Common/AppJsonContext.cs`.
3. **CLI Arguments & Commands**:
   - Powered by `ConsoleAppFramework` v5.
   - Commands are defined as static methods with XML document comments for parameter descriptions and short flag aliases.
4. **Output Guidelines**:
   - Do not print directly to `Console.WriteLine` or `AnsiConsole.MarkupLine` in command logic.
   - Use `ConsoleOutput.Render` to cleanly handle both human-facing Spectre.Console tables/panels and machine-facing `--json` structured outputs.
5. **Testing**:
   - Unit tests are located in `tests/unit/`, integration tests in `tests/integration/`.
   - Use `TestConsoleContext` to capture and verify output in tests.
