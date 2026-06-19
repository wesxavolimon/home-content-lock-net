using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;

namespace HomeContentLock.Presentation.Commands;

/// <summary>
/// Command to enable the content blocker.
/// Usage: blocker enable --password <password>
/// </summary>
public class EnableCommand : BaseCommand
{
    private readonly Option<string> _passwordOption;

    public EnableCommand(ServiceProvider? serviceProvider = null)
        : base("enable", "Enable the content blocker", serviceProvider)
    {
        _passwordOption = new Option<string>(
            new[] { "--password", "-p" },
            description: "Password required to enable the blocker");
        _passwordOption.IsRequired = true;

        AddOption(_passwordOption);
        this.SetHandler(ExecuteAsync, _passwordOption);
    }

    private async Task ExecuteAsync(string password)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                Console.WriteLine("Error: Password cannot be empty");
                Environment.Exit(1);
                return;
            }

            var service = GetBlockerService();
            var result = await service.EnableAsync(password);

            if (!result.IsSuccess)
            {
                Console.WriteLine($"Error: {result.Message}");
                Environment.Exit(1);
                return;
            }

            Console.WriteLine("✓ Content blocker enabled successfully");
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fatal error: {ex.Message}");
            Environment.Exit(1);
        }
    }
}
