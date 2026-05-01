using SharedKernel;

namespace Tsutskiridze.TradeBuddy.API.Telegram.Errors;

public class TelegramPresentationException:BaseException
{
    public TelegramPresentationException(string message, int statusCode = 400, Exception? inner = null) : base(message, statusCode, inner)
    {
    }
}