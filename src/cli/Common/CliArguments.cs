namespace Cli.Common;

/// <summary>
/// Inspects raw command-line arguments before ConsoleAppFramework parses them, so that
/// framework-level failures (bad values, unknown flags, unknown commands) honor --json too.
/// </summary>
public static class CliArguments
{
    /// <summary>
    /// Returns true when --json or -j appears before an optional "--" terminator.
    /// </summary>
    public static bool IsJsonMode(IReadOnlyList<string> args)
    {
        foreach (var arg in args)
        {
            if (arg == "--")
            {
                break;
            }

            if (arg is "--json" or "-j")
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Returns the first argument when it names no registered command, or null otherwise.
    /// No arguments, a leading --help/-h/--version, or a registered root command are left to the framework.
    /// </summary>
    public static string? FindUnknownCommand(IReadOnlyList<string> args, IEnumerable<string> commandNames)
    {
        if (args.Count == 0 || args[0] is "--help" or "-h" or "--version")
        {
            return null;
        }

        var topLevelCommands = new HashSet<string>(StringComparer.Ordinal);
        foreach (var name in commandNames)
        {
            if (name.Length == 0)
            {
                return null;
            }

            topLevelCommands.Add(name.Split(' ')[0]);
        }

        return topLevelCommands.Contains(args[0]) ? null : args[0];
    }
}
