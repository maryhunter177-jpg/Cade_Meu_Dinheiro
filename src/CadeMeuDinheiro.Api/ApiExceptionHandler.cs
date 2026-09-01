using CadeMeuDinheiro.Application;
using CadeMeuDinheiro.Domain;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CadeMeuDinheiro.Api;

public sealed partial class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, type, title) = exception switch
        {
            AppValidationException or DomainException => (400, "validation_error", exception.Message),
            NotFoundException => (404, "not_found", exception.Message),
            ConflictException => (409, "conflict", exception.Message),
            UnauthorizedAccessException => (401, "unauthorized", "Sua sessão não é válida."),
            _ => (500, "unexpected_error", "Não foi possível concluir a operação.")
        };
        if (status >= 500) LogUnhandled(logger, httpContext.TraceIdentifier, exception);
        var details = new ProblemDetails { Status = status, Type = type, Title = title, Instance = httpContext.Request.Path };
        details.Extensions["traceId"] = httpContext.TraceIdentifier;
        if (exception is AppValidationException validation) details.Extensions["errors"] = validation.Errors;
        httpContext.Response.StatusCode = status;
        await httpContext.Response.WriteAsJsonAsync(details, cancellationToken);
        return true;
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "Unhandled API error. TraceId: {TraceId}")]
    private static partial void LogUnhandled(ILogger logger, string traceId, Exception exception);
}

public static class HttpContextExtensions
{
    public static Guid UserId(this HttpContext context)
    {
        var value = context.User.FindFirst("sub")?.Value;
        return Guid.TryParse(value, out var id) ? id : throw new UnauthorizedAccessException();
    }
}
