using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;

namespace HomeContentLock.Presentation.Commands;

/// <summary>
/// Command group for managing blocked sites.
/// Usage:
///   blocker sites add <domain>
///   blocker sites remove <domain>
///   blocker sites list
/// </summary>
public class SitesCommand : Command
{
    private readonly ServiceProvider _serviceProvider;

    public SitesCommand(ServiceProvider? serviceProvider = null)
        : base("sites", "Manage blocked sites")
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

        AddCommand(new AddSiteCommand(_serviceProvider));
        AddCommand(new RemoveSiteCommand(_serviceProvider));
        AddCommand(new ListSitesCommand(_serviceProvider));
    }
}

/// <summary>
/// Subcommand to add a site to the block list.
/// </summary>
public class AddSiteCommand : BaseCommand
{
    private readonly Argument<string> _domainArgument;

    public AddSiteCommand(ServiceProvider? serviceProvider = null)
        : base("add", "Add a site to the block list", serviceProvider)
    {
        _domainArgument = new Argument<string>("domain", "Domain to block (e.g., example.com)");
        AddArgument(_domainArgument);
        this.SetHandler(ExecuteAsync);
    }

    private async Task ExecuteAsync(string domain)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                Console.WriteLine("Error: Domain cannot be empty");
                Environment.Exit(1);
                return;
            }

            var service = GetBlockerService();
            var result = await service.AddCustomSiteAsync(domain);

            if (!result.IsSuccess)
            {
                Console.WriteLine($"Error: {result.Message}");
                Environment.Exit(1);
                return;
            }

            Console.WriteLine($"✓ {result.Message}");
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fatal error: {ex.Message}");
            Environment.Exit(1);
        }
    }
}

/// <summary>
/// Subcommand to remove a site from the block list.
/// </summary>
public class RemoveSiteCommand : BaseCommand
{
    private readonly Argument<string> _domainArgument;

    public RemoveSiteCommand(ServiceProvider? serviceProvider = null)
        : base("remove", "Remove a site from the block list", serviceProvider)
    {
        _domainArgument = new Argument<string>("domain", "Domain to unblock");
        AddArgument(_domainArgument);
        this.SetHandler(ExecuteAsync);
    }

    private async Task ExecuteAsync(string domain)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                Console.WriteLine("Error: Domain cannot be empty");
                Environment.Exit(1);
                return;
            }

            var service = GetBlockerService();
            var result = await service.RemoveCustomSiteAsync(domain);

            if (!result.IsSuccess)
            {
                Console.WriteLine($"Error: {result.Message}");
                Environment.Exit(1);
                return;
            }

            Console.WriteLine($"✓ {result.Message}");
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fatal error: {ex.Message}");
            Environment.Exit(1);
        }
    }
}

/// <summary>
/// Subcommand to list all blocked sites.
/// </summary>
public class ListSitesCommand : BaseCommand
{
    public ListSitesCommand(ServiceProvider? serviceProvider = null)
        : base("list", "List all blocked sites", serviceProvider)
    {
        this.SetHandler(ExecuteAsync);
    }

    private async Task ExecuteAsync()
    {
        try
        {
            var service = GetBlockerService();
            var result = await service.QueryLogsAsync("SITE_ADDED", 1000);

            if (!result.IsSuccess)
            {
                Console.WriteLine($"Error: {result.Message}");
                Environment.Exit(1);
                return;
            }

            var logs = result.Data!;

            if (logs.Count == 0)
            {
                Console.WriteLine("No custom blocked sites found.");
                Environment.Exit(0);
                return;
            }

            Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                   Custom Blocked Sites                           ║");
            Console.WriteLine("╠════════════════════════════════════════════════════════════════╣");

            var sites = new HashSet<string>();
            foreach (var log in logs)
            {
                if (!string.IsNullOrEmpty(log.Details))
                {
                    // Extract domain from details (format: "Custom site added: example.com")
                    var parts = log.Details.Split(": ");
                    if (parts.Length == 2)
                    {
                        sites.Add(parts[1]);
                    }
                }
            }

            foreach (var site in sites.OrderBy(s => s))
            {
                Console.WriteLine($"║ • {site.PadRight(60)} ║");
            }

            Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
            Console.WriteLine($"Total: {sites.Count} sites");
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fatal error: {ex.Message}");
            Environment.Exit(1);
        }
    }
}
