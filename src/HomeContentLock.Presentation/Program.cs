using System.CommandLine;
using HomeContentLock.CrossCutting;
using HomeContentLock.Application.UseCases.GetStatus;
using HomeContentLock.Application.UseCases.EnableBlocker;
using HomeContentLock.Application.UseCases.DisableBlocker;
using HomeContentLock.Application.UseCases.QueryLogs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Serilog;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

try
{
    var services = new ServiceCollection();
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    
    services.AddApplicationServices(connectionString);
    services.AddLogging(builder => builder.AddSerilog());

    var serviceProvider = services.BuildServiceProvider();

    var rootCommand = new RootCommand("HomeContentLock - Content Blocker CLI");

    var statusCommand = new Command("status", "Get current blocker status")
    {
        Handler = System.CommandLine.Invocation.CommandHandler.Create(async () =>
        {
            var useCase = serviceProvider.GetRequiredService<GetStatusUseCase>();
            var status = await useCase.ExecuteAsync();
            Console.WriteLine(\$"Status: {status}\");
        })
    };

    var enableCommand = new Command("enable", "Enable content blocker")
    {
        Handler = System.CommandLine.Invocation.CommandHandler.Create(async () =>
        {
            var useCase = serviceProvider.GetRequiredService<EnableBlockerUseCase>();
            await useCase.ExecuteAsync();
            Console.WriteLine("Blocker enabled");
        })
    };

    var disableCommand = new Command("disable", "Disable content blocker")
    {
        Handler = System.CommandLine.Invocation.CommandHandler.Create(async () =>
        {
            var useCase = serviceProvider.GetRequiredService<DisableBlockerUseCase>();
            await useCase.ExecuteAsync();
            Console.WriteLine("Blocker disabled");
        })
    };

    var logsCommand = new Command("logs", "View activity logs")
    {
        Handler = System.CommandLine.Invocation.CommandHandler.Create(async () =>
        {
            var useCase = serviceProvider.GetRequiredService<QueryLogsUseCase>();
            var logs = await useCase.ExecuteAsync(50);
            foreach (var log in logs)
            {
                Console.WriteLine(\$"[{log.Timestamp:u}] {log.Action}: {log.Status} - {log.Details}\");
            }
        })
    };

    rootCommand.AddCommand(statusCommand);
    rootCommand.AddCommand(enableCommand);
    rootCommand.AddCommand(disableCommand);
    rootCommand.AddCommand(logsCommand);

    return await rootCommand.InvokeAsync(args);
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}
