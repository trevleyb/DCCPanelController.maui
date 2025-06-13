using System.Collections.ObjectModel;

namespace DCCClient.Helpers;

public interface IResult {
    bool IsSuccess { get; }
    bool IsFailure { get; }
    string? Message => Messages.FirstOrDefault();
    IError? Error => Errors?.FirstOrDefault();
    IReadOnlyCollection<string> Messages { get; }
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
    private readonly List<Exception> _causes = new();

    public string Message { get; } = message;
    public Exception? Exception { get; } = exception;

    public IError CausedBy(Exception exception) {
        _causes.Add(exception);
        return this;
    }

    public override string ToString() {
        var causes = _causes.Count != 0
            ? $"Caused by: {string.Join(", ", _causes.Select(e => e.Message))}"
            : string.Empty;
        return $"{Message} {causes}".Trim();
    }
}

// Builder interfaces
public interface IResultBuilder {
    IResultBuilder WithMessage(string message);
    IResultBuilder WithMessages(IEnumerable<string> messages);
    IResultBuilder WithMessages(params string[] messages);
    IResult Build();
}

public interface IResultBuilder<T> {
    IResultBuilder<T> WithValue(T? value);
    IResultBuilder<T> WithMessage(string message);
    IResultBuilder<T> WithMessages(IEnumerable<string> messages);
    IResultBuilder<T> WithMessages(params string[] messages);
    IResult<T> Build();
}

public interface IFailureResultBuilder {
    IFailureResultBuilder WithError(string message);
    IFailureResultBuilder WithError(IError error);
    IFailureResultBuilder WithError(Exception exception);
    IFailureResultBuilder WithError(string message, Exception exception);
    IFailureResultBuilder WithErrors(IEnumerable<string> messages);
    IFailureResultBuilder WithErrors(IEnumerable<IError> errors);
    IFailureResultBuilder WithErrors(IEnumerable<Exception> exceptions);
    IFailureResultBuilder WithErrors(params string[] messages);
    IResult Build();
}

public interface IFailureResultBuilder<T> {
    IFailureResultBuilder<T> WithError(string message);
    IFailureResultBuilder<T> WithError(IError error);
    IFailureResultBuilder<T> WithError(Exception exception);
    IFailureResultBuilder<T> WithError(string message, Exception exception);
    IFailureResultBuilder<T> WithErrors(IEnumerable<string> messages);
    IFailureResultBuilder<T> WithErrors(IEnumerable<IError> errors);
    IFailureResultBuilder<T> WithErrors(IEnumerable<Exception> exceptions);
    IFailureResultBuilder<T> WithErrors(params string[] messages);
    IResult<T> Build();
}

// Builder implementations
internal class ResultBuilder : IResultBuilder {
    private readonly List<string> _messages = new();

    public IResultBuilder WithMessage(string message) {
        _messages.Add(message);
        return this;
    }

    public IResultBuilder WithMessages(IEnumerable<string> messages) {
        _messages.AddRange(messages);
        return this;
    }

    public IResultBuilder WithMessages(params string[] messages) {
        _messages.AddRange(messages);
        return this;
    }

    public IResult Build() {
        return new Result(true, _messages.Count > 0 ? _messages : null, null);
    }
}

internal class ResultBuilder<T> : IResultBuilder<T> {
    private readonly List<string> _messages = new();
    private T? _value = default;

    public IResultBuilder<T> WithValue(T? value) {
        _value = value;
        return this;
    }

    public IResultBuilder<T> WithMessage(string message) {
        _messages.Add(message);
        return this;
    }

    public IResultBuilder<T> WithMessages(IEnumerable<string> messages) {
        _messages.AddRange(messages);
        return this;
    }

    public IResultBuilder<T> WithMessages(params string[] messages) {
        _messages.AddRange(messages);
        return this;
    }

    public IResult<T> Build() {
        return new Result<T>(true, _value, _messages.Count > 0 ? _messages : null, null);
    }
}

internal class FailureResultBuilder : IFailureResultBuilder {
    private readonly List<IError> _errors = new();

    public IFailureResultBuilder WithError(string message) {
        _errors.Add(new Error(message));
        return this;
    }

    public IFailureResultBuilder WithError(IError error) {
        _errors.Add(error);
        return this;
    }

    public IFailureResultBuilder WithError(Exception exception) {
        _errors.Add(new Error(exception.Message, exception));
        return this;
    }

    public IFailureResultBuilder WithError(string message, Exception exception) {
        _errors.Add(new Error(message, exception));
        return this;
    }

    public IFailureResultBuilder WithErrors(IEnumerable<string> messages) {
        _errors.AddRange(messages.Select(msg => new Error(msg)));
        return this;
    }

    public IFailureResultBuilder WithErrors(IEnumerable<IError> errors) {
        _errors.AddRange(errors);
        return this;
    }

    public IFailureResultBuilder WithErrors(IEnumerable<Exception> exceptions) {
        _errors.AddRange(exceptions.Select(e => new Error(e.Message, e)));
        return this;
    }

    public IFailureResultBuilder WithErrors(params string[] messages) {
        _errors.AddRange(messages.Select(msg => new Error(msg)));
        return this;
    }

    public IResult Build() {
        return new Result(false, null, _errors);
    }
}

internal class FailureResultBuilder<T> : IFailureResultBuilder<T> {
    private readonly List<IError> _errors = new();

    public IFailureResultBuilder<T> WithError(string message) {
        _errors.Add(new Error(message));
        return this;
    }

    public IFailureResultBuilder<T> WithError(IError error) {
        _errors.Add(error);
        return this;
    }

    public IFailureResultBuilder<T> WithError(Exception exception) {
        _errors.Add(new Error(exception.Message, exception));
        return this;
    }

    public IFailureResultBuilder<T> WithError(string message, Exception exception) {
        _errors.Add(new Error(message, exception));
        return this;
    }

    public IFailureResultBuilder<T> WithErrors(IEnumerable<string> messages) {
        _errors.AddRange(messages.Select(msg => new Error(msg)));
        return this;
    }

    public IFailureResultBuilder<T> WithErrors(IEnumerable<IError> errors) {
        _errors.AddRange(errors);
        return this;
    }

    public IFailureResultBuilder<T> WithErrors(IEnumerable<Exception> exceptions) {
        _errors.AddRange(exceptions.Select(e => new Error(e.Message, e)));
        return this;
    }

    public IFailureResultBuilder<T> WithErrors(params string[] messages) {
        _errors.AddRange(messages.Select(msg => new Error(msg)));
        return this;
    }

    public IResult<T> Build() {
        return new Result<T>(false, default, null, _errors);
    }
}

public class Result<T> : IResult<T> {
    internal Result(bool isSuccess, T? value, IEnumerable<string>? messages, IEnumerable<IError>? errors) {
        IsSuccess = isSuccess;
        Value = value;
        Messages = new ReadOnlyCollection<string>(messages?.ToList() ?? []);
        Errors = new ReadOnlyCollection<IError>(errors?.ToList() ?? []);
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T? Value { get; }
    public IReadOnlyCollection<string> Messages { get; }
    public IReadOnlyCollection<IError> Errors { get; }

    // Builder entry points
    public static IResultBuilder<T> Success() => new ResultBuilder<T>();
    public static IFailureResultBuilder<T> Failure() => new FailureResultBuilder<T>();

    // Error factory method for fluent chaining
    public static IError Error(string message) => new Error(message);

    // Original static factory methods (backward compatibility)
    public static IResult<T> Ok(T? value = default, string? message = null) => new Result<T>(true, value, message != null ? [message] : null, null);

    public static IResult<T> Ok(T? value, IEnumerable<string> messages) => new Result<T>(true, value, messages, null);

    public static IResult<T> Fail(string message) => new Result<T>(false, default, null, [new Error(message)]);

    public static IResult<T> Fail(IError error) => new Result<T>(false, default, null, [error]);

    public static IResult<T> Fail(IEnumerable<string> messages) => new Result<T>(false, default, null, messages.Select(msg => new Error(msg)));

    public static IResult<T> Fail(IEnumerable<IError> errors) => new Result<T>(false, default, null, errors);

    public static IResult<T> Fail(IEnumerable<Exception> exceptions) => new Result<T>(false, default, null, exceptions.Select(e => new Error(e.Message, e)));

    public static IResult<T> Fail(Exception exception) => new Result<T>(false, default, null, [new Error(exception.Message, exception)]);

    public static IResult<T> Fail(string message, Exception exception) => new Result<T>(false, default, null, [new Error(message, exception)]);

    public override string ToString() {
        if (IsSuccess) {
            var messageText = Messages.Any() ? string.Join(", ", Messages) : Value?.ToString();
            return messageText != null ? $"Success: {messageText}" : "Success";
        }
        return $"Failure: {string.Join(", ", Errors)}";
    }
}

public class Result : IResult {
    internal Result(bool isSuccess, IEnumerable<string>? messages, IEnumerable<IError>? errors) {
        IsSuccess = isSuccess;
        Messages = new ReadOnlyCollection<string>(messages?.ToList() ?? []);
        Errors = new ReadOnlyCollection<IError>(errors?.ToList() ?? []);
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public IReadOnlyCollection<string> Messages { get; }
    public IReadOnlyCollection<IError> Errors { get; }

    // Builder entry points
    public static IResultBuilder Success() => new ResultBuilder();
    public static IFailureResultBuilder Failure() => new FailureResultBuilder();

    // Error factory method for fluent chaining
    public static IError Error(string message) => new Error(message);

    // Original static factory methods (backward compatibility)
    public static IResult Ok(string? message = null) => new Result(true, message != null ? [message] : null, null);

    public static IResult Ok(IEnumerable<string> messages) => new Result(true, messages, null);

    public static IResult Fail(string message) => new Result(false, null, [new Error(message)]);

    public static IResult Fail(IError error) => new Result(false, null, [error]);

    public static IResult Fail(IEnumerable<string> messages) => new Result(false, null, messages.Select(msg => new Error(msg)));

    public static IResult Fail(IEnumerable<IError> errors) => new Result(false, null, errors);

    public static IResult Fail(IEnumerable<Exception> exceptions) => new Result(false, null, exceptions.Select(e => new Error(e.Message, e)));

    public static IResult Fail(Exception exception) => new Result(false, null, [new Error(exception.Message, exception)]);

    public static IResult Fail(string message, Exception exception) => new Result(false, null, [new Error(message, exception)]);

    public override string ToString() {
        if (IsSuccess) {
            return Messages.Any() ? $"Success: {string.Join(", ", Messages)}" : "Success";
        }
        return $"Failure: {string.Join(", ", Errors)}";
    }
}