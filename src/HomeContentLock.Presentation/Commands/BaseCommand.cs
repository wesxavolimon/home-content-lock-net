using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using HomeContentLock.Application.Services;

namespace HomeContentLock.Presentation.Commands;

/// <summary>
/// Base class for all CLI commands with dependency injection support.
/// </summary>
public abstract class BaseCommand : Command
{
    protected readonly ServiceProvider ServiceProvider;

    protected BaseCommand(string name, string? description = null, ServiceProvider? serviceProvider = null)
        : base(name, description)
    {
        ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    protected BlockerService GetBlockerService()
    {
        var service = ServiceProvider.GetService<BlockerService>();
        if (service == null)
            throw new InvalidOperationException("BlockerService not registered");
        return service;
    }
}
