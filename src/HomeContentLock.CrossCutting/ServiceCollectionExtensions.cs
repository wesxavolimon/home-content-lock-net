using HomeContentLock.Domain.Interfaces;
using HomeContentLock.Infrastructure.Persistence;
using HomeContentLock.Infrastructure.Services;
using HomeContentLock.Application.UseCases.GetStatus;
using HomeContentLock.Application.UseCases.EnableBlocker;
using HomeContentLock.Application.UseCases.DisableBlocker;
using HomeContentLock.Application.UseCases.QueryLogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HomeContentLock.CrossCutting;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<BlockerDatabase>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<IBlockerRepository, SqliteBlockerRepository>();
        services.AddScoped<IPasswordValidator, PasswordValidator>();

        services.AddScoped<GetStatusUseCase>();
        services.AddScoped<EnableBlockerUseCase>();
        services.AddScoped<DisableBlockerUseCase>();
        services.AddScoped<QueryLogsUseCase>();

        return services;
    }
}
