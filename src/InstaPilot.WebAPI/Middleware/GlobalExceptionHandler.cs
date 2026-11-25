using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
namespace InstaPilot.WebAPI.Middleware;
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled request exception. TraceId: {TraceId}", context.TraceIdentifier);
        var problem = new ProblemDetails { Status = StatusCodes.Status500InternalServerError, Title = "خطای غیرمنتظره در سرور رخ داد.", Detail = context.RequestServices.GetRequiredService<IHostEnvironment>().IsDevelopment() ? exception.Message : "لطفاً بعداً دوباره تلاش کنید." };
        problem.Extensions["traceId"] = context.TraceIdentifier; context.Response.StatusCode = problem.Status!.Value;
        await context.Response.WriteAsJsonAsync(problem, cancellationToken); return true;
    }
}
