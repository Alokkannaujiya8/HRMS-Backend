using Hangfire.Dashboard;
using System.Text;

namespace HRMS.API.HangfireSupport
{
    public class HangfireDashboardBasicAuthFilter : IDashboardAuthorizationFilter
    {
        private readonly string _username;
        private readonly string _password;

        public HangfireDashboardBasicAuthFilter(IConfiguration configuration)
        {
            _username = configuration["Hangfire:Dashboard:Username"] ?? "admin";
            _password = configuration["Hangfire:Dashboard:Password"] ?? "admin123";
        }

        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();
            var header = httpContext.Request.Headers.Authorization.ToString();

            if (string.IsNullOrWhiteSpace(header) || !header.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            {
                Challenge(httpContext);
                return false;
            }

            try
            {
                var encoded = header.Substring("Basic ".Length).Trim();
                var credentialBytes = Convert.FromBase64String(encoded);
                var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':', 2);

                if (credentials.Length != 2)
                {
                    Challenge(httpContext);
                    return false;
                }

                var isValid = string.Equals(credentials[0], _username, StringComparison.Ordinal) &&
                              string.Equals(credentials[1], _password, StringComparison.Ordinal);

                if (!isValid)
                {
                    Challenge(httpContext);
                }

                return isValid;
            }
            catch
            {
                Challenge(httpContext);
                return false;
            }
        }

        private static void Challenge(HttpContext context)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.Headers.Append("WWW-Authenticate", "Basic realm=\"Hangfire Dashboard\"");
        }
    }
}
