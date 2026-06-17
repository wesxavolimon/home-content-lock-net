using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace HomeContentLock.Presentation.Commands;

/// <summary>
/// Command to display blocker logs.
/// Usage: blocker logs [--lines 50] [--action ENABLE] [--output json|table]
/// </summary>
public class LogsCommand : BaseCommand
{
    private readonly Option<int> _linesOption;
    private readonly Option<string?> _actionOption;
    private readonly Option<string> _outputOption;

    public LogsCommand(ServiceProvider? serviceProvider = null)
        : base("logs", "Show blocker activity logs", serviceProvider)
    {
        _linesOption = new Option<int>(
            new[] { "--lines", "-n" },
            getDefaultValue: () => 50,
            description: "Number of log entries to show (default: 50)");

        _actionOption = new Option<string?>(
            new[] { "--action", "-a" },
            description: "Filter by action (e.g., ENABLE, DISABLE)");

        _outputOption = new Option<string>(
            new[] { "--output", "-o" },
            getDefaultValue: () => "table",
            description: "Output format: json or table (default: table)");

        AddOption(_linesOption);
        AddOption(_actionOption);
        AddOption(_outputOption);

        this.SetHandler(ExecuteAsync);
    }

    private async Task ExecuteAsync(int lines, string? action, string output, InvocationContext context)
    {
        try
        {
            var service = GetBlockerService();
            var result = await service.QueryLogsAsync(action, lines);

            if (!result.IsSuccess)
            {
                Console.WriteLine($"Error: {result.Message}");
                context.ExitCode = 1;
                return;
            }

            if (output.Equals("json", StringComparison.OrdinalIgnoreCase))
            {
                PrintAsJson(result.Data!);
            }
            else
            {
                PrintAsTable(result.Data!);
            }

            context.ExitCode = 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fatal error: {ex.Message}");
            context.ExitCode = 1;
        }
    }

    private static void PrintAsTable(List<HomeContentLock.Application.UseCases.LogEntryDto> logs)
    {
        if (logs.Count == 0)
        {
            Console.WriteLine("No logs found.");
            return;
        }

        Console.WriteLine("╔════════════════════════════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║ Timestamp                    │ Action             │ Status     │ Details               ║");
        Console.WriteLine("╠════════════════════════════════════════════════════════════════════════════════════════╣");

        foreach (var log in logs.OrderByDescending(l => l.Timestamp))
        {
            var timestamp = log.Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
            var action = (log.Action ?? "").PadRight(18);
            var status = (log.Status ?? "").PadRight(10);
            var details = (log.Details ?? "").Length > 21 ? (log.Details[..21] + "..") : (log.Details ?? "").PadRight(21);

            Console.WriteLine($"║ {timestamp} │ {action} │ {status} │ {details} ║");
        }

        Console.WriteLine("╚════════════════════════════════════════════════════════════════════════════════════════╝");
        Console.WriteLine($"Total: {logs.Count} entries");
    }

    private static void PrintAsJson(List<HomeContentLock.Application.UseCases.LogEntryDto> logs)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(logs, options);
        Console.WriteLine(json);
    }
}
