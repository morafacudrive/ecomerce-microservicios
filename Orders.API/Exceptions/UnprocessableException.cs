namespace Orders.API.Exceptions;

public class UnprocessableException(string errorCode, string message)
    : Exception(message)
{
    public string ErrorCode { get; } = errorCode;
}