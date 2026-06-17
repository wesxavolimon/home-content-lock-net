using System.CommandLine;
using System.CommandLine.Invocation;
using Microsoft.Extensions.DependencyInjection;

namespace HomeContentLock.Presentation.Commands;

/// <summary>
/// Command to disable the content blocker.
/// Usage: blocker disable --password <password>
/// </summary>
public class DisableCommand : BaseCommand
{
    private readonly Option<string?> _passwordOption;

    public DisableCommand(ServiceProvider? serviceProvider = null)
        : base("disable", "Disable the content blocker", serviceProvider)
    {
        _passwordOption = new Option<string?>(
            new[] { "--password", "-p" },
            description: "Password required to disable the blocker");

        AddOption(_passwordOption);
        this.SetHandler(ExecuteAsync);
    }

    private async Task ExecuteAsync(string? password, InvocationContext context)
    {
        try
        {
            // If password not provided via option, prompt user
            if (string.IsNullOrWhiteSpace(password))
            {
                Console.Write("Enter password to disable blocker: ");
                password = ReadPasswordFromConsole();

                if (string.IsNullOrWhiteSpace(password))
                {
                    Console.WriteLine("Error: Password cannot be empty");
                    context.ExitCode = 1;
                    return;
                }
            }

            var service = GetBlockerService();
            var result = await service.DisableAsync(password);

            if (!result.IsSuccess)
            {
                Console.WriteLine($"Error: {result.Message}");
                context.ExitCode = 1;
                return;
            }

            Console.WriteLine("✓ Content blocker disabled successfully");
            context.ExitCode = 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fatal error: {ex.Message}");
            context.ExitCode = 1;
        }
    }

    private static string ReadPasswordFromConsole()
    {
        var password = string.Empty;
        ConsoleKey key;

        do
        {
            var keyInfo = Console.ReadKey(intercept: true);
            key = keyInfo.Key;

            if (key == ConsoleKey.Backspace)
            {
                if (password.Length > 0)
                {
                    password = password[0..^1];
                    Console.Write("\b \b");
                }
            }
            else if (key != ConsoleKey.Enter)
            {
                password += keyInfo.KeyChar;
                Console.Write("*");
            }
        } while (key != ConsoleKey.Enter);

        Console.WriteLine();
        return password;
    }
}
