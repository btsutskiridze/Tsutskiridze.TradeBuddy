using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;
using SharedKernel.Validations;
using Tsutskiridze.TradeBuddy.Application.Common.Exceptions;
using Tsutskiridze.TradeBuddy.Infrastructure.Exceptions;

namespace Tsutskiridze.TradeBuddy.API.Exceptions;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, type, message, errors) = exception switch
        {
            ValidationException ex => (400, "VALIDATION_ERROR", ex.Message, ex.Errors),
            DomainException ex => (ex.StatusCode, "DOMAIN_RULE_VIOLATION", ex.Message, null),
            ResourceNotFoundException ex => (ex.StatusCode, "NOT_FOUND", ex.Message, null),
            ApplicationLayerException ex => (ex.StatusCode, "APPLICATION_ERROR", ex.Message, null),
            InfrastructureException ex => (ex.StatusCode, "SERVICE_UNAVAILABLE", ex.Message, null),
            UnauthorizedAccessException ex => (401, "UNAUTHORIZED", ex.Message, null),
            _ => (500, "INTERNAL_ERROR", "Internal Server Error", null)
        };

        if (status == 500)
        {
            _logger.LogError(exception, "Unhandled exception");
        }

        var problemDetails = CreateProblemDetails(status, type, message, errors);
        problemDetails.Extensions.Add("traceId", GetStackTraceId(httpContext));

        httpContext.Response.ContentType = "application/problem+json";
        httpContext.Response.StatusCode = status;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private static ProblemDetails CreateProblemDetails(int status, string type, string message,
        IReadOnlyList<ValidationError>? errors)
    {
        if (errors == null || !errors.Any())
        {
            return new ProblemDetails
            {
                Type = type,
                Detail = message,
                Status = status,
            };
        }

        var errorsDict = errors
            .GroupBy(x => x.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.ErrorMessage).ToArray()
            );

        return new ValidationProblemDetails(errorsDict)
        {
            Type = type,
            Detail = message,
            Status = status,
        };
    }

    private string GetStackTraceId(HttpContext httpContext)
    {
        return Activity.Current?.Id ?? httpContext.TraceIdentifier;
    }
}