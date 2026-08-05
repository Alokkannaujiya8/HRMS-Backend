using HRMS.Application.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net;
using System.Text.Json;

namespace HRMS.API.Middleware
{
    /// <summary>
    /// Enterprise Global Exception Middleware enforcing RFC7807 ProblemDetails standards.
    /// </summary>
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
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
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
            var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

            _logger.LogError(
                exception,
                "Exception caught in Global Middleware. TraceId: {TraceId}, Method: {Method}, Path: {Path}",
                traceId,
                context.Request.Method,
                context.Request.Path);

            var (statusCode, problemDetails) = GetProblemDetails(context, exception, traceId);

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json";

            var json = JsonSerializer.Serialize(problemDetails, problemDetails.GetType(), new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });

            await context.Response.WriteAsync(json);
        }

        private (int StatusCode, ProblemDetails Details) GetProblemDetails(HttpContext context, Exception exception, string traceId)
        {
            return exception switch
            {
                HRMS.Application.Common.Exceptions.ValidationException valEx => (
                    StatusCodes.Status400BadRequest,
                    CreateValidationProblemDetails(context, valEx, traceId)
                ),

                NotFoundException notFoundEx => (
                    StatusCodes.Status404NotFound,
                    CreateProblemDetails(context, StatusCodes.Status404NotFound, "Resource Not Found", notFoundEx.Message, notFoundEx.ErrorCode, traceId)
                ),

                BusinessException busEx => (
                    StatusCodes.Status400BadRequest,
                    CreateProblemDetails(context, StatusCodes.Status400BadRequest, "Business Rule Violation", busEx.Message, busEx.ErrorCode, traceId)
                ),

                UnauthorizedException unauthEx => (
                    StatusCodes.Status401Unauthorized,
                    CreateProblemDetails(context, StatusCodes.Status401Unauthorized, "Unauthorized Access", unauthEx.Message, unauthEx.ErrorCode, traceId)
                ),

                ForbiddenException verbEx => (
                    StatusCodes.Status403Forbidden,
                    CreateProblemDetails(context, StatusCodes.Status403Forbidden, "Forbidden Access", verbEx.Message, verbEx.ErrorCode, traceId)
                ),

                BaseException baseEx => (
                    StatusCodes.Status400BadRequest,
                    CreateProblemDetails(context, StatusCodes.Status400BadRequest, "Application Error", baseEx.Message, baseEx.ErrorCode, traceId)
                ),

                _ => (
                    StatusCodes.Status500InternalServerError,
                    CreateProblemDetails(
                        context,
                        StatusCodes.Status500InternalServerError,
                        "Internal Server Error",
                        _environment.IsDevelopment() ? exception.ToString() : "An unexpected server error occurred.",
                        "ERROR.UNHANDLED",
                        traceId)
                )
            };
        }

        private ProblemDetails CreateProblemDetails(
            HttpContext context,
            int statusCode,
            string title,
            string detail,
            string errorCode,
            string traceId)
        {
            var details = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Instance = context.Request.Path
            };

            details.Extensions["errorCode"] = errorCode;
            details.Extensions["traceId"] = traceId;
            details.Extensions["timestamp"] = DateTime.UtcNow.ToString("o");

            return details;
        }

        private ValidationProblemDetails CreateValidationProblemDetails(
            HttpContext context,
            HRMS.Application.Common.Exceptions.ValidationException valEx,
            string traceId)
        {
            var details = new ValidationProblemDetails(valEx.Errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Failed",
                Detail = valEx.Message,
                Instance = context.Request.Path
            };

            details.Extensions["errorCode"] = valEx.ErrorCode;
            details.Extensions["traceId"] = traceId;
            details.Extensions["timestamp"] = DateTime.UtcNow.ToString("o");

            return details;
        }
    }
}
