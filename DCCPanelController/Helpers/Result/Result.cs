namespace DCCPanelController.Helpers.Result;

public class Result<T> {
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T? Value { get; }
    public string? Error { get; }

    protected Result(bool isSuccess, T? value, string? error) {
        switch (isSuccess) {
        case true when value == null:
            throw new ArgumentNullException(nameof(value), "Success result must have a value.");
        case true when !string.IsNullOrEmpty(error):
            throw new ArgumentException("Success result cannot have an error message.", nameof(error));
        case false when string.IsNullOrEmpty(error):
            throw new ArgumentException("Failure result must have an error message.", nameof(error));
        }

        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static Result<T> Success(T value) {
        return new Result<T>(true, value, null);
    }

    public static Result<T> Failure(string? error) {
        return new Result<T>(false, default(T) , error);
    }
}