using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace HRMS.API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IHostEnvironment _environment;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger,
            IHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var statusCode = exception switch
            {
                InvalidOperationException => StatusCodes.Status400BadRequest,
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                KeyNotFoundException => StatusCodes.Status404NotFound,
                _ => StatusCodes.Status500InternalServerError
            };

            _logger.LogError(
                exception,
                "Unhandled exception while processing {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = statusCode == (int)HttpStatusCode.InternalServerError
                    ? "An unexpected error occurred."
                    : exception.Message,
                Detail = _environment.IsDevelopment() ? exception.ToString() : null,
                Instance = context.Request.Path
            };

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json";

            var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }
    }
}
