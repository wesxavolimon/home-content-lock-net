using HomeContentLock.Domain.Entities;
using HomeContentLock.Domain.Interfaces;
using HomeContentLock.Application.Results;
using Serilog;

namespace HomeContentLock.Application.UseCases;

/// <summary>
/// Use case for adding a custom site to the block list.
/// </summary>
public class AddCustomSiteUseCase
{
    private readonly IBlockerRepository _repository;
    private readonly ILogger _logger;

    public AddCustomSiteUseCase(IBlockerRepository repository, ILogger logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Execute the use case.
    /// </summary>
    public async Task<Result> ExecuteAsync(string domain)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                _logger.Warning("Add custom site attempted with empty domain");
                return Result.Failure("Domain cannot be empty");
            }

            domain = domain.ToLowerInvariant().Trim();

            // Basic domain validation
            if (!IsValidDomain(domain))
            {
                _logger.Warning("Add custom site attempted with invalid domain: {Domain}", domain);
                return Result.Failure("Invalid domain format");
            }

            var site = new CustomBlockedSite
            {
                Domain = domain,
                AddedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _repository.SaveCustomSiteAsync(site);

            var log = new LogEntry
            {
                Timestamp = DateTime.UtcNow,
                Action = "SITE_ADDED",
                Status = "SUCCESS",
                Details = $"Custom site added: {domain}"
            };

            await _repository.SaveLogAsync(log);

            _logger.Information("Custom site added: {Domain}", domain);
            return Result.Success($"Site '{domain}' added to block list");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error adding custom site");
            return Result.Failure("Failed to add custom site", ex);
        }
    }

    private static bool IsValidDomain(string domain)
    {
        // Basic domain validation
        if (string.IsNullOrWhiteSpace(domain))
            return false;

        // Must contain at least one dot (e.g., example.com)
        if (domain.Count(c => c == '.') < 1 && domain != "localhost")
            return false;

        // Cannot start or end with dot
        if (domain.StartsWith(".") || domain.EndsWith("."))
            return false;

        // Cannot contain spaces
        if (domain.Contains(" "))
            return false;

        // Allowed characters: alphanumeric, dots, hyphens, wildcards
        var allowedChars = "abcdefghijklmnopqrstuvwxyz0123456789.-*";
        return domain.All(c => allowedChars.Contains(c));
    }
}
