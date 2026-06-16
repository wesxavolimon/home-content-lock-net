namespace HomeContentLock.Application.Results;

/// <summary>
/// Base result class for all use case operations.
/// Provides a standard way to handle success and failure scenarios.
/// </summary>
public class Result
{
    public bool IsSuccess { get; protected set; }
    public string Message { get; protected set; } = string.Empty;
    public Exception? Exception { get; protected set; }

    protected Result(bool isSuccess, string message = "", Exception? exception = null)
    {
        IsSuccess = isSuccess;
        Message = message;
        Exception = exception;
    }

    public static Result Success(string message = "Operation completed successfully")
        => new Result(true, message);

    public static Result Failure(string message, Exception? exception = null)
        => new Result(false, message, exception);

    public static Result<T> Success<T>(T data, string message = "Operation completed successfully")
        => new Result<T>(true, data, message);

    public static Result<T> Failure<T>(string message, Exception? exception = null)
        => new Result<T>(false, default!, message, exception);
}

/// <summary>
/// Generic result class for operations that return data.
/// </summary>
public class Result<T> : Result
{
    public T? Data { get; protected set; }

    protected internal Result(bool isSuccess, T? data, string message = "", Exception? exception = null)
        : base(isSuccess, message, exception)
    {
        Data = data;
    }
}

/// <summary>
/// Result for status queries.
/// </summary>
public class StatusResult : Result<BlockerStatusDto>
{
    public StatusResult(BlockerStatusDto data)
        : base(true, data, "Status retrieved successfully")
    {
    }

    public static StatusResult Failure(string message)
        => new StatusResult(null!) { IsSuccess = false, Message = message };
}

/// <summary>
/// DTO for blocker status
/// </summary>
public class BlockerStatusDto
{
    public string State { get; set; } = "DISABLED";
    public int LogCount { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
