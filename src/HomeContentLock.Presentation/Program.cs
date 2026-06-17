using System.CommandLine;
using System.CommandLine.Invocation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using HomeContentLock.Domain.Interfaces;
using HomeContentLock.Infrastructure.Persistence;
using HomeContentLock.Infrastructure.Security;
using HomeContentLock.Application.Services;
using HomeContentLock.Application.UseCases;
using HomeContentLock.Presentation.Commands;
using Serilog;

namespace HomeContentLock.Presentation;

class Program
{
    static async Task<int> Main(string[] args)
    {
        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                path: Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    ".config/HomeContentLock/logs/blocker-.txt"),
                rollingInterval: RollingInterval.Day,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        try
        {
            // Setup dependency injection
            var services = new ServiceCollection();
            ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();

            // Ensure database is created
            using (var dbContext = serviceProvider.GetRequiredService<BlockerDbContext>())
            {
                await dbContext.Database.EnsureCreatedAsync();
            }

            // Setup CLI
            var root = BuildRootCommand(serviceProvider);
            return await root.InvokeAsync(args);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "An unexpected error occurred");
            return 1;
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }

    static void ConfigureServices(ServiceCollection services)
    {
        var logger = Log.Logger;

        // Register DbContext
        var dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".config/HomeContentLock/blocker.db");

        services.AddDbContext<BlockerDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        // Register Infrastructure
        services.AddSingleton(logger);
        services.AddScoped<IBlockerRepository, SqliteBlockerRepository>();
        services.AddScoped<SecretsFile>();
        services.AddScoped<IPasswordValidator, FilePasswordValidator>();

        // Register Use Cases
        services.AddScoped<GetStatusUseCase>();
        services.AddScoped<EnableBlockerUseCase>();
        services.AddScoped<DisableBlockerUseCase>();
        services.AddScoped<QueryLogsUseCase>();
        services.AddScoped<SetPasswordUseCase>();
        services.AddScoped<AddCustomSiteUseCase>();
        services.AddScoped<RemoveCustomSiteUseCase>();

        // Register Application Service
        services.AddScoped<BlockerService>();
    }

    static RootCommand BuildRootCommand(ServiceProvider serviceProvider)
    {
        var root = new RootCommand("HomeContentLock - Content Blocker CLI")
        {
            TreatUnmatchedTokensAsErrors = true
        };

        // Register commands
        root.AddCommand(new StatusCommand(serviceProvider));
        root.AddCommand(new EnableCommand(serviceProvider));
        root.AddCommand(new DisableCommand(serviceProvider));
        root.AddCommand(new LogsCommand(serviceProvider));
        root.AddCommand(new PasswordCommand(serviceProvider));
        root.AddCommand(new SitesCommand(serviceProvider));

        return root;
    }
}
