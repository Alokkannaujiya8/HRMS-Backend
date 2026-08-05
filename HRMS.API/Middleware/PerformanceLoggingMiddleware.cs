using System.Diagnostics;

namespace HRMS.API.Middleware
{
    /// <summary>
    /// Middleware for logging request details, timing execution performance, and warning on slow requests (> 500ms).
    /// </summary>
    public class PerformanceLoggingMiddleware
    {
        private const int PerformanceThresholdMs = 500;
        private readonly RequestDelegate _next;
        private readonly ILogger<PerformanceLoggingMiddleware> _logger;

        public PerformanceLoggingMiddleware(RequestDelegate next, ILogger<PerformanceLoggingMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var request = context.Request;
            _logger.LogInformation(
                "HTTP Request Started: {Method} {Path}{QueryString} [ContentType: {ContentType}, ContentLength: {ContentLength}]",
                request.Method,
                request.Path,
                request.QueryString.HasValue ? request.QueryString.Value : string.Empty,
                request.ContentType ?? "N/A",
                request.ContentLength.HasValue ? request.ContentLength.Value : 0);

            var timer = Stopwatch.StartNew();

            try
            {
                await _next(context);
                timer.Stop();

                var elapsedMs = timer.ElapsedMilliseconds;
                var statusCode = context.Response.StatusCode;

                if (elapsedMs > PerformanceThresholdMs)
                {
                    _logger.LogWarning(
                        "SLOW REQUEST WARNING: HTTP {Method} {Path} completed in {ElapsedMs}ms with Status {StatusCode}",
                        request.Method,
                        request.Path,
                        elapsedMs,
                        statusCode);
                }
                else
                {
                    _logger.LogInformation(
                        "HTTP Request Completed: {Method} {Path} responded {StatusCode} in {ElapsedMs}ms",
                        request.Method,
                        request.Path,
                        statusCode,
                        elapsedMs);
                }
            }
            catch (Exception ex)
            {
                timer.Stop();
                _logger.LogError(
                    ex,
                    "HTTP Request Failed: {Method} {Path} threw exception after {ElapsedMs}ms",
                    request.Method,
                    request.Path,
                    timer.ElapsedMilliseconds);
                throw;
            }
        }
    }
}
