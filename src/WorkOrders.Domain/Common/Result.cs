namespace WorkOrders.Domain.Common;

/// <summary>
/// The outcome of an operation that can fail in an expected way: a success, or an error that
/// says why not. Callers must check <see cref="IsSuccess"/> before they continue.
/// </summary>
public class Result
{
    private static readonly Result SuccessResult = new(error: null);

    private readonly Error? _error;

    protected Result(Error? error) => _error = error;

    public bool IsSuccess => _error is null;

    public bool IsFailure => !IsSuccess;

    /// <summary>Why the operation failed. Throws when the result is a success.</summary>
    public Error Error => _error ?? throw new InvalidOperationException("A successful result has no error.");

    public static Result Success() => SuccessResult;

    public static Result Failure(Error error) => new(error ?? throw new ArgumentNullException(nameof(error)));

    public static Result<TValue> Success<TValue>(TValue value) => new(value);

    public static Result<TValue> Failure<TValue>(Error error) =>
        new(error ?? throw new ArgumentNullException(nameof(error)));

    public static implicit operator Result(Error error) => Failure(error);
}

/// <summary>The outcome of an operation that produces a value when it succeeds.</summary>
public sealed class Result<TValue> : Result
{
    private readonly TValue? _value;

    internal Result(TValue value)
        : base(error: null) => _value = value;

    internal Result(Error error)
        : base(error)
    {
    }

    /// <summary>The value of a successful result. Throws when the result is a failure.</summary>
    public TValue Value => IsSuccess ? _value! : throw new InvalidOperationException("A failed result has no value.");

    public static implicit operator Result<TValue>(TValue value) => Success(value);

    public static implicit operator Result<TValue>(Error error) => Failure<TValue>(error);
}
