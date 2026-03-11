namespace SharedKernel;

public abstract class BaseException : Exception
{
    public int StatusCode {get; private set;}
    
    public BaseException(string message, int statusCode = 400, Exception? inner = null) : base(message, inner)
    {
        if (statusCode is < 400 or >= 500)
        {
            throw new ArgumentOutOfRangeException(nameof(statusCode));
        }
        
        StatusCode = statusCode;
    }
}