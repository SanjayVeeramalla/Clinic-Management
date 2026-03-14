namespace ClinicManagementAPI.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        _logger.LogInformation("Request: {Method} {Path} | IP: {IP}",
            context.Request.Method, context.Request.Path,
            context.Connection.RemoteIpAddress);

        await _next(context);

        _logger.LogInformation("Response: {StatusCode} for {Method} {Path}",
            context.Response.StatusCode, context.Request.Method, context.Request.Path);
    }
}