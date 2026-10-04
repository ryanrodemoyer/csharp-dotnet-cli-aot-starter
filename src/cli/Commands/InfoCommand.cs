using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cli.Common;
using Cli.Models;
using Spectre.Console;

namespace Cli.Commands;

/// <summary>
/// Handles the 'info' command.
/// Inspects system environment, memory usage, and Native AOT runtime characteristics.
/// </summary>
public static class InfoCommand
{
    /// <summary>
    /// Display system diagnostics, runtime characteristics, and Native AOT status.
    /// </summary>
    /// <param name="json">-j, Output structured JSON for machine or agent consumption</param>
    public static int Execute(bool json = false)
    {
        var asm = typeof(InfoCommand).Assembly;
        var appName = asm.GetName().Name ?? "cli";
        var appVersion = asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                         ?? asm.GetName().Version?.ToString()
                         ?? "0.1.0";

        var isNativeAot = !RuntimeFeature.IsDynamicCodeCompiled;
        var isDynamicCodeSupported = RuntimeFeature.IsDynamicCodeSupported;

        long workingSetBytes;
        try
        {
            using var proc = Process.GetCurrentProcess();
            workingSetBytes = proc.WorkingSet64;
        }
        catch
        {
            workingSetBytes = GC.GetTotalMemory(forceFullCollection: false);
        }

        var workingSetMb = workingSetBytes / (1024.0 * 1024.0);
        var formattedWorkingSet = $"{workingSetMb.ToString("F2", CultureInfo.InvariantCulture)} MB";

        var info = new SystemInfoData(
            ApplicationName: appName,
            ApplicationVersion: appVersion,
            DotNetVersion: Environment.Version.ToString(),
            IsNativeAot: isNativeAot,
            IsDynamicCodeSupported: isDynamicCodeSupported,
            OsDescription: RuntimeInformation.OSDescription,
            OsArchitecture: RuntimeInformation.OSArchitecture.ToString(),
            ProcessArchitecture: RuntimeInformation.ProcessArchitecture.ToString(),
            ProcessorCount: Environment.ProcessorCount,
            WorkingSetBytes: workingSetBytes,
            FormattedWorkingSet: formattedWorkingSet,
            IsServerGc: System.Runtime.GCSettings.IsServerGC);

        ConsoleOutput.Render(
            jsonMode: json,
            data: info,
            typeInfo: AppJsonContext.Default.CliResultSystemInfoData,
            renderHuman: (d, console) =>
            {
                var table = new Table()
                    .Border(TableBorder.Rounded)
                    .Title($"[bold yellow]{Markup.Escape(d.ApplicationName)} Diagnostics[/]")
                    .AddColumn(new TableColumn("[bold]Property[/]").LeftAligned())
                    .AddColumn(new TableColumn("[bold]Value[/]").LeftAligned());

                var aotBadge = d.IsNativeAot
                    ? "[bold green]YES[/] (Ahead-Of-Time Native Machine Code)"
                    : "[bold yellow]NO[/] (JIT / Managed IL)";

                table.AddRow("Application Version", Markup.Escape(d.ApplicationVersion));
                table.AddRow(".NET Runtime Version", Markup.Escape(d.DotNetVersion));
                table.AddRow("Native AOT Active", aotBadge);
                table.AddRow("Dynamic Code Supported", d.IsDynamicCodeSupported ? "Yes" : "No");
                table.AddRow("Operating System", Markup.Escape(d.OsDescription));
                table.AddRow("OS Architecture", Markup.Escape(d.OsArchitecture));
                table.AddRow("Process Architecture", Markup.Escape(d.ProcessArchitecture));
                table.AddRow("Logical Processors", d.ProcessorCount.ToString(CultureInfo.InvariantCulture));
                table.AddRow("Memory Working Set", Markup.Escape(d.FormattedWorkingSet));
                table.AddRow("Garbage Collector", d.IsServerGc ? "Server GC" : "Workstation GC");

                console.Write(table);
            });

        return 0;
    }
}
