using System.Collections.ObjectModel;

namespace DCCClients.Common;

public interface IResult {
    bool IsSuccess { get; }
    bool IsFailure { get; }
    string? Message { get; }
    IReadOnlyCollection<IError> Errors { get; }
    IError? Error => Errors?.FirstOrDefault();
}

public interface IResult<T> : IResult {
    T? Value { get; }
}

public interface IError {
    string Message { get; }
    Exception? Exception { get; }
    IError CausedBy(Exception exception);
}

public class Error(string message, Exception? exception = null) : IError {
    private List<Exception> Causes { get; } = new();
    public string Message { get; } = message;
    public Exception? Exception { get; } = exception;

    public IError CausedBy(Exception exception) {
        Causes.Add(exception);
        return this;
    }

    public override string ToString() {
        var causes = Causes.Count != 0 ? $"Caused by: {string.Join(", ", Causes.Select(e => e.Message))}" : string.Empty;
        return $"{Message} {causes}".Trim();
    }
}

// Generic result with or without a value
public class Result<T> : IResult<T> {
    protected Result(bool isSuccess, T? value, string? message, IEnumerable<IError>? errors) {
        IsSuccess = isSuccess;
        Value = value;
        Message = message;
        Errors = new ReadOnlyCollection<IError>(errors?.ToList() ?? []);
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T? Value { get; }
    public string? Message { get; }
    public IReadOnlyCollection<IError> Errors { get; }

    // Static factory methods for success
    public static IResult<T> Ok(T? value = default, string? message = null) {
        return new Result<T>(true, value, message, null);
    }

    // Static factory methods for failure
    public static IResult<T> Fail(string message) {
        return new Result<T>(false, default, null, [new Error(message)]);
    }

    public static IResult<T> Fail(IError error) {
        return new Result<T>(false, default, null, [error]);
    }

    public static IResult<T> Fail(IEnumerable<string> messages) {
        return new Result<T>(false, default, null, messages.Select(msg => new Error(msg)));
    }

    public static IResult<T> Fail(IEnumerable<IError>? errors) {
        return new Result<T>(false, default, null, errors);
    }

    public static IResult<T> Fail(IEnumerable<Exception>? exceptions) {
        return new Result<T>(false, default, null, exceptions?.Select(e => new Error(e.Message, e)));
    }

    public static IResult<T> Fail(Exception exception) {
        return new Result<T>(false, default, null, [new Error(exception.Message, exception)]);
    }

    public static IResult<T> Fail(string message, Exception exception) {
        return new Result<T>(false, default, null, [new Error(message, exception)]);
    }

    public override string ToString() {
        if (IsSuccess) {
            return Message != null ? $"Success: {Message}" : $"Success: {Value}";
        }
        return $"Failure: {string.Join(", ", Errors)}";
    }
}

// Non-generic Result for cases where no data needs to be returned
public class Result : IResult {
    private readonly IReadOnlyCollection<IError> _errors;
    
    private Result(bool isSuccess, string? message, IEnumerable<IError>? errors) { 
        IsSuccess = isSuccess;
        Message = message;
        _errors = new ReadOnlyCollection<IError>(errors?.ToList() ?? []);
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Message { get; }
    public IReadOnlyCollection<IError> Errors => _errors;

    // Static methods for success and failure
    public static IResult Ok(string? message = null) {
        return new Result(true, message, null);
    }

    public static IResult Fail(string message) {
        return new Result(false, null, [new Error(message)]);
    }

    public static IResult Fail(IError error) {
        return new Result(false, null, [error]);
    }

    public static IResult Fail(IEnumerable<string> messages) {
        return new Result(false, null, messages.Select(msg => new Error(msg)));
    }

    public static IResult Fail(IEnumerable<IError>? errors) {
        return new Result(false, null, errors);
    }

    public static IResult Fail(IEnumerable<Exception>? exceptions) {
        return new Result(false, null, exceptions?.Select(e => new Error(e.Message, e)));
    }

    public static IResult Fail(Exception exception) {
        return new Result(false, null, [new Error(exception.Message, exception)]);
    }

    public static IResult Fail(string message, Exception exception) {
        return new Result(false, null, [new Error(message, exception)]);
    }
    
    public override string ToString() {
        if (IsSuccess) {
            return Message != null ? $"Success: {Message}" : "Success";
        }
        return $"Failure: {string.Join(", ", Errors)}";
    }
}