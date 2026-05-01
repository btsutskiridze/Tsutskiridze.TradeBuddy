namespace SharedKernel;

public class DomainException : BaseException
{
    public DomainException(string message, int statusCode = 409, Exception? inner = null) : base(message, statusCode, inner)
    {
    }
}