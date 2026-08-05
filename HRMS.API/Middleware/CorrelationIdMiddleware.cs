using Serilog.Context;

namespace HRMS.API.Middleware
{
    /// <summary>
    /// Middleware for extracting or generating X-Correlation-ID HTTP header and enriching Serilog LogContext.
    /// </summary>
    public class CorrelationIdMiddleware
    {
        public const string CorrelationIdHeaderKey = "X-Correlation-ID";
        private readonly RequestDelegate _next;

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var correlationId = GetOrGenerateCorrelationId(context);

            context.TraceIdentifier = correlationId;

            context.Response.OnStarting(() =>
            {
                if (!context.Response.Headers.ContainsKey(CorrelationIdHeaderKey))
                {
                    context.Response.Headers.Append(CorrelationIdHeaderKey, correlationId);
                }
                return Task.CompletedTask;
            });

            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                await _next(context);
            }
        }

        private static string GetOrGenerateCorrelationId(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue(CorrelationIdHeaderKey, out var headerValue) &&
                !string.IsNullOrWhiteSpace(headerValue))
            {
                return headerValue.ToString();
            }

            return Guid.NewGuid().ToString("N");
        }
    }
}
