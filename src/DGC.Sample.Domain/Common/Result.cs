using DGC.Sample.Domain.Exceptions.Errors;

namespace DGC.Sample.Domain.Common;

public sealed class Result<TValue>
{
    private readonly TValue? _value;
    private readonly AzureError? _error;

    private Result(TValue value)
    {
        IsSuccess = true;
        _value = value;
        _error = null;
    }

    private Result(AzureError error)
    {
        IsSuccess = false;
        _value = default;
        _error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    public TValue Value => IsSuccess 
        ? _value! 
        : throw new InvalidOperationException("The value of a failure result can not be accessed.");

    public AzureError Error => !IsSuccess 
        ? _error! 
        : throw new InvalidOperationException("The error of a success result can not be accessed.");

    public static Result<TValue> Success(TValue value) => new(value);
    public static Result<TValue> Failure(AzureError error) => new(error);
    public static Result<TValue> Failure(string code, string message) => new(new AzureError(code, message));

    public static implicit operator Result<TValue>(TValue value) => Success(value);
    public static implicit operator Result<TValue>(AzureError error) => Failure(error);
}

public sealed class Result
{
    private readonly AzureError? _error;

    private Result(bool isSuccess, AzureError? error)
    {
        IsSuccess = isSuccess;
        _error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    public AzureError Error => !IsSuccess 
        ? _error! 
        : throw new InvalidOperationException("The error of a success result can not be accessed.");

    public static Result Success() => new(true, null);
    public static Result Failure(AzureError error) => new(false, error);
    public static Result Failure(string code, string message) => new(false, new AzureError(code, message));

    public static implicit operator Result(AzureError error) => Failure(error);
}
