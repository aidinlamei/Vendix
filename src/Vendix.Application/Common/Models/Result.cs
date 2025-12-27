namespace Vendix.Application.Common.Models;

/// <summary>
/// Represents the result of an operation that can either succeed or fail.
/// </summary>
/// <typeparam name="T">The type of the value returned on success.</typeparam>
public class Result<T>
{
    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the value returned on success.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when accessing Value on a failed result.</exception>
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access Value on a failed result.");

    /// <summary>
    /// Gets the error message on failure.
    /// </summary>
    public string? Error { get; }

    private readonly T? _value;

    private Result(bool isSuccess, T? value, string? error)
    {
        IsSuccess = isSuccess;
        _value = value;
        Error = error;
    }

    /// <summary>
    /// Creates a successful result with the specified value.
    /// </summary>
    /// <param name="value">The value to return.</param>
    /// <returns>A successful result containing the value.</returns>
    public static Result<T> Success(T value) => new(true, value, null);

    /// <summary>
    /// Creates a failed result with the specified error message.
    /// </summary>
    /// <param name="error">The error message.</param>
    /// <returns>A failed result containing the error.</returns>
    public static Result<T> Failure(string error) => new(false, default, error);

    /// <summary>
    /// Implicitly converts a value to a successful result.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator Result<T>(T value) => Success(value);

    /// <summary>
    /// Maps the result value to a new type if successful.
    /// </summary>
    /// <typeparam name="TNew">The new type.</typeparam>
    /// <param name="mapper">The mapping function.</param>
    /// <returns>A new result with the mapped value or the original error.</returns>
    public Result<TNew> Map<TNew>(Func<T, TNew> mapper)
    {
        return IsSuccess
            ? Result<TNew>.Success(mapper(_value!))
            : Result<TNew>.Failure(Error!);
    }

    /// <summary>
    /// Binds the result to a new result if successful.
    /// </summary>
    /// <typeparam name="TNew">The new type.</typeparam>
    /// <param name="binder">The binding function.</param>
    /// <returns>The new result or a failed result with the original error.</returns>
    public Result<TNew> Bind<TNew>(Func<T, Result<TNew>> binder)
    {
        return IsSuccess
            ? binder(_value!)
            : Result<TNew>.Failure(Error!);
    }

    /// <summary>
    /// Matches the result and executes the appropriate function.
    /// </summary>
    /// <typeparam name="TResult">The return type.</typeparam>
    /// <param name="onSuccess">Function to execute on success.</param>
    /// <param name="onFailure">Function to execute on failure.</param>
    /// <returns>The result of the executed function.</returns>
    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, TResult> onFailure)
    {
        return IsSuccess
            ? onSuccess(_value!)
            : onFailure(Error!);
    }
}

/// <summary>
/// Represents the result of an operation that can either succeed or fail, without a return value.
/// </summary>
public class Result
{
    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the error message on failure.
    /// </summary>
    public string? Error { get; }

    private Result(bool isSuccess, string? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <returns>A successful result.</returns>
    public static Result Success() => new(true, null);

    /// <summary>
    /// Creates a failed result with the specified error message.
    /// </summary>
    /// <param name="error">The error message.</param>
    /// <returns>A failed result containing the error.</returns>
    public static Result Failure(string error) => new(false, error);

    /// <summary>
    /// Matches the result and executes the appropriate function.
    /// </summary>
    /// <typeparam name="TResult">The return type.</typeparam>
    /// <param name="onSuccess">Function to execute on success.</param>
    /// <param name="onFailure">Function to execute on failure.</param>
    /// <returns>The result of the executed function.</returns>
    public TResult Match<TResult>(Func<TResult> onSuccess, Func<string, TResult> onFailure)
    {
        return IsSuccess
            ? onSuccess()
            : onFailure(Error!);
    }
}
