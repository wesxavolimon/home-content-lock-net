using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;

namespace HomeContentLock.Presentation.Commands;

/// <summary>
/// Command to set or update the blocker password.
/// Usage: blocker password
/// </summary>
public class PasswordCommand : BaseCommand
{
    public PasswordCommand(ServiceProvider? serviceProvider = null)
        : base("password", "Set or update the blocker password", serviceProvider)
    {
        this.SetHandler(ExecuteAsync);
    }

    private async Task ExecuteAsync()
    {
        try
        {
            Console.Write("Enter new password: ");
            var password = ReadPasswordFromConsole();

            if (string.IsNullOrWhiteSpace(password))
            {
                Console.WriteLine("Error: Password cannot be empty");
                Environment.Exit(1);
                return;
            }

            Console.Write("Confirm password: ");
            var confirmPassword = ReadPasswordFromConsole();

            if (password != confirmPassword)
            {
                Console.WriteLine("Error: Passwords do not match");
                Environment.Exit(1);
                return;
            }

            var service = GetBlockerService();
            var result = await service.SetPasswordAsync(password);

            if (!result.IsSuccess)
            {
                Console.WriteLine($"Error: {result.Message}");
                Environment.Exit(1);
                return;
            }

            Console.WriteLine("✓ Password updated successfully");
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fatal error: {ex.Message}");
            Environment.Exit(1);
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
