namespace MicrosoftOrleans.Domain.Shared;

public class Result<T>
{
    public T? Value { get; }
    public string? ErrorMessage { get; }
    public bool IsSuccess { get; }

    private Result(T value)
    {
        Value = value;
        IsSuccess = true;
    }

    private Result(string errorMessage)
    {
        ErrorMessage = errorMessage;
        IsSuccess = false;
    }

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(string errorMessage) => new(errorMessage);
}
