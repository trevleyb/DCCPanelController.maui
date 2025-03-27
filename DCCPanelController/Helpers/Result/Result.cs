using System.Collections.ObjectModel;

namespace DCCPanelController.Helpers.Result;

public interface IResult {
    bool IsSuccess { get; }
    bool IsFailure { get; }
    IReadOnlyCollection<IError> Errors { get; }
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
    public string Message { get; } = message;
    public Exception? Exception { get; } = exception;
    private List<Exception> Causes { get; } = new();

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
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T? Value { get; }
    public IReadOnlyCollection<IError> Errors { get; }

    protected Result(bool isSuccess, T? value, IEnumerable<IError>? errors) {
        IsSuccess = isSuccess;
        Value = value;
        Errors = new ReadOnlyCollection<IError>(errors?.ToList() ?? []);
    }

    // Static factory methods for success
    public static IResult<T?> Ok(T? value = default) {
        return new Result<T?>(true, value, null);
    }

    // Static factory methods for failure
    public static Result<T?> Fail(string message) {
        return new Result<T?>(false, default, [new Error(message)]);
    }

    public static Result<T?> Fail(IError error) {
        return new Result<T?>(false, default, [error]);
    }

    public static Result<T?> Fail(IEnumerable<string> messages) {
        return new Result<T?>(false, default, messages.Select(msg => new Error(msg)));
    }

    public static Result<T?> Fail(IEnumerable<IError>? errors) {
        return new Result<T?>(false, default, errors);
    }

    public static Result<T?> Fail(IEnumerable<Exception>? exceptions) {
        return new Result<T?>(false, default, exceptions?.Select(e => new Error(e.Message, e)));
    }

    public static Result<T?> Fail(Exception exception) {
        return new Result<T?>(false, default, [new Error(exception.Message, exception)]);
    }

    public static Result<T?> Fail(string message, Exception exception) {
        return new Result<T?>(false, default, [new Error(message, exception)]);
    }

    public override string ToString() {
        return IsSuccess ? $"Success: {Value}" : $"Failure: {string.Join(", ", Errors)}";
    }
}

// Non-generic Result for cases where no data needs to be returned
public class Result : Result<object> {
    private Result(bool isSuccess, IEnumerable<IError>? errors)
        : base(isSuccess, null, errors) { }

    // Static methods for success and failure
    public static Result Ok() {
        return new Result(true, null);
    }

    public new static Result Fail(string message) {
        return new Result(false, [new Error(message)]);
    }

    public new static IResult Fail(IError error) {
        return new Result(false, [error]);
    }

    public new static Result Fail(IEnumerable<string> messages) {
        return new Result(false, messages.Select(msg => new Error(msg)));
    }

    public new static Result Fail(IEnumerable<IError>? errors) {
        return new Result(false, errors);
    }
    
    public new static Result Fail(IEnumerable<Exception>? exceptions) {
        return new Result(false, exceptions?.Select(e => new Error(e.Message, e)));
    }

    public new static Result Fail(Exception exception) {
        return new Result(false, [new Error(exception.Message, exception)]);
    }

    public new static Result Fail(string message, Exception exception) {
        return new Result(false, [new Error(message, exception)]);
    }
}



// namespace DCCPanelController.Helpers.Result;
//
// public class Result<T> {
//     protected Result(bool isSuccess, T? value, string? error) {
//         switch (isSuccess) {
//         case true when value == null:
//             throw new ArgumentNullException(nameof(value), "Success result must have a value.");
//
//         case true when !string.IsNullOrEmpty(error):
//             throw new ArgumentException("Success result cannot have an error message.", nameof(error));
//
//         case false when string.IsNullOrEmpty(error):
//             throw new ArgumentException("Failure result must have an error message.", nameof(error));
//         }
//
//         IsSuccess = isSuccess;
//         Value = value;
//         Error = error;
//     }
//
//     public bool IsSuccess { get; }
//     public bool IsFailure => !IsSuccess;
//     public T? Value { get; }
//     public string? Error { get; }
//
//     public static Result<T> Success(T value) {
//         return new Result<T>(true, value, null);
//     }
//
//     public static Result<T> Failure(string? error) {
//         return new Result<T>(false, default, error);
//     }
// }