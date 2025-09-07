using System;

namespace DCCCommon;

public enum ResultStatus { Ok, Failed, Error }

public interface IResult {
    ResultStatus Status { get; }
    bool IsOk { get; }
    bool IsSuccess { get; }
    bool IsFailed { get; }
    bool IsFailure { get; }
    bool IsError { get; }
    string? Message { get; }
    Exception? Exception { get; }
}

// Covariant generic interface (so Result<Derived> fits where IResult<Base> is expected)
public interface IResult<out T> : IResult {
    T? Value { get; }
}

// -------------------- Non-generic --------------------
public sealed record Result(ResultStatus Status, string? Message = null, Exception? Exception = null) : IResult {
    public bool IsOk => Status == ResultStatus.Ok;
    public bool IsSuccess => Status == ResultStatus.Ok;
    public bool IsFailed => Status is ResultStatus.Failed or ResultStatus.Error;
    public bool IsFailure => Status is ResultStatus.Failed or ResultStatus.Error;
    public bool IsError => Status is ResultStatus.Error or ResultStatus.Failed;

    // Factories (all return new instances)
    public static Result Ok(string? message = null) => new(ResultStatus.Ok, message);
    public static Result Failed(string message) => new(ResultStatus.Failed, message);
    public static Result Fail(string message) => new(ResultStatus.Failed, message);
    public static Result Fail(Exception ex, string? message) => new(ResultStatus.Failed, message, ex);
    public static Result Error(Exception ex, string? message = null) => new(ResultStatus.Error, message, ex);

    // Fluent “with” helpers (return new instances)
    public Result WithMessage(string? message) => this with { Message = message };
    public Result WithException(Exception? ex) => this with { Exception = ex };

    // Lift to generic while preserving state
    public Result<T> As<T>(T? value = default) => new(Status, Message, Exception, value);
}

// -------------------- Generic --------------------
public sealed record Result<T>(
    ResultStatus Status,
    string? Message = null,
    Exception? Exception = null,
    T? Value = default) : IResult<T> {
    public bool IsOk => Status == ResultStatus.Ok;
    public bool IsSuccess => Status == ResultStatus.Ok;
    public bool IsFailed => Status is ResultStatus.Failed or ResultStatus.Error;
    public bool IsFailure => Status is ResultStatus.Failed or ResultStatus.Error;
    public bool IsError => Status is ResultStatus.Error or ResultStatus.Failed;

    // Factories (all return new instances)
    public static Result<T> Ok(T? value = default, string? message = null) => new(ResultStatus.Ok, message, null, value);
    public static Result<T> Failed(string message) => new(ResultStatus.Failed, message);
    public static Result<T> Fail(string message) => new(ResultStatus.Failed, message);
    public static Result<T> Fail(Exception ex, string? message) => new(ResultStatus.Failed, message, ex);
    public static Result<T> Error(Exception ex, string? message = null) => new(ResultStatus.Error, message, ex);
    
    // Fluent “with” helpers (all return new instances)
    public Result<T> WithValue(T? value) => this with { Value = value };
    public Result<T> WithMessage(string? message) => this with { Message = message };
    public Result<T> WithException(Exception? ex) => this with { Exception = ex };

    // Map/Bind (functional niceties) — also immutable
    public Result<U> Map<U>(Func<T?, U?> mapper) => mapper is null
        ? new(ResultStatus.Error, "Mapper was null", new ArgumentNullException(nameof(mapper)))
        : new(Status, Message, Exception, mapper(Value));

    public Result<U> Bind<U>(Func<T?, Result<U>>? binder) {
        if (binder is null) {
            return new Result<U>(ResultStatus.Error, "Binder was null", new ArgumentNullException(nameof(binder)));
        }

        var next = binder(Value);

        // Carry forward message/exception if downstream left them empty
        if (string.IsNullOrEmpty(next.Message) && !string.IsNullOrEmpty(Message))
            next = next.WithMessage(Message);
        if (next.Exception is null && Exception is not null)
            next = next.WithException(Exception);

        return next with { Status = Status == ResultStatus.Error ? ResultStatus.Error : next.Status };
    }

    // Drop the value, keep the status/message/exception
    public Result WithoutValue() => new(Status, Message, Exception);
}