using HomeContentLock.Domain.Entities;
using HomeContentLock.Domain.Interfaces;
using HomeContentLock.Domain.Exceptions;
using HomeContentLock.Application.Results;
using Serilog;

namespace HomeContentLock.Application.UseCases;

/// <summary>
/// Use case for enabling the content blocker.
/// Requires valid password authentication.
/// </summary>
public class EnableBlockerUseCase
{
    private readonly IBlockerRepository _repository;
    private readonly IPasswordValidator _validator;
    private readonly ILogger _logger;

    public EnableBlockerUseCase(IBlockerRepository repository, IPasswordValidator validator, ILogger logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Execute the use case.
    /// </summary>
    public async Task<Result> ExecuteAsync(string password)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                _logger.Warning("Enable blocker attempted with empty password");
                throw new InvalidPasswordException("Password is required");
            }

            var isValid = await _validator.ValidateAsync(password);
            if (!isValid)
            {
                _logger.Warning("Enable blocker attempted with invalid password");
                throw new InvalidPasswordException("Invalid password");
            }

            var log = new LogEntry
            {
                Timestamp = DateTime.UtcNow,
                Action = "ENABLE",
                Status = "SUCCESS",
                Details = "Content blocker enabled by user"
            };

            await _repository.SaveLogAsync(log);
            await _repository.UpdateStatusAsync(Domain.Entities.BlockerStatus.Enabled);

            _logger.Information("Content blocker enabled successfully");
            return Result.Success("Content blocker enabled successfully");
        }
        catch (InvalidPasswordException ex)
        {
            _logger.Error(ex, "Password validation failed for enable operation");
            return Result.Failure(ex.Message, ex);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error enabling content blocker");
            return Result.Failure("Failed to enable content blocker", ex);
        }
    }
}
