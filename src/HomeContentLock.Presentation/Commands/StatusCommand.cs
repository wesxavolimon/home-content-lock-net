using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;

namespace HomeContentLock.Presentation.Commands;

/// <summary>
/// Command to display the current status of the blocker.
/// Usage: blocker status
/// </summary>
public class StatusCommand : BaseCommand
{
    public StatusCommand(ServiceProvider? serviceProvider = null)
        : base("status", "Show the current blocker status", serviceProvider)
    {
        this.SetHandler(ExecuteAsync);
    }

    private async Task ExecuteAsync()
    {
        try
        {
            var service = GetBlockerService();
            var result = await service.GetStatusAsync();

            if (!result.IsSuccess)
            {
                Console.WriteLine($"Error: {result.Message}");
                Environment.Exit(1);
                return;
            }

            Console.WriteLine("╔════════════════════════════════════════╗");
            Console.WriteLine("║       HomeContentLock - Status          ║");
            Console.WriteLine("╠════════════════════════════════════════╣");
            Console.WriteLine($"║ State: {result.Data!.State.PadRight(33)} ║");
            Console.WriteLine($"║ Logs: {result.Data.LogCount.ToString().PadRight(33)} ║");
            Console.WriteLine($"║ Last Updated: {result.Data.LastUpdated:yyyy-MM-dd HH:mm:ss}    ║");
            Console.WriteLine("╚════════════════════════════════════════╝");

            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fatal error: {ex.Message}");
            Environment.Exit(1);
        }
    }
}
