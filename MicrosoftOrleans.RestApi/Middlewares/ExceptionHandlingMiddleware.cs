using MicrosoftOrleans.Domain.Shared;
using Serilog;
using System.Net;
using System.Text.Json;

namespace MicrosoftOrleans.RestApi.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OrleansMessageRejectionException ex)
        {
            Log.Error(ex, "An orleans exception occurred.");
            await HandleExceptionAsync(context, ex);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An unhandled exception occurred.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var result = Result<object>.Failure(exception.Message);
        return context.Response.WriteAsync(JsonSerializer.Serialize(result));
    }
}