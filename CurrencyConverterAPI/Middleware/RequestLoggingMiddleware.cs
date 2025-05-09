namespace CurrencyConverterAPI.Middleware
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            var startTime = DateTime.UtcNow;

            // Extract client IP address
            var clientIp = httpContext.Connection.RemoteIpAddress?.ToString();

            // Extract ClientId from JWT Token (assuming JWT token is in the Authorization header)
            var clientId = httpContext.User.Claims.FirstOrDefault(c => c.Type == "ClientId")?.Value;

            // Extract HTTP method & target endpoint
            var method = httpContext.Request.Method;
            var endpoint = httpContext.Request.Path;
            _logger.LogInformation("Request started: {Method} {Endpoint} from {ClientIp} for ClientId {ClientId}", method, endpoint, clientIp, clientId);

            // Proceed with the request
            await _next(httpContext);

            var endTime = DateTime.UtcNow;
            var responseTime = (endTime - startTime).TotalMilliseconds;

            // Extract response code
            var responseCode = httpContext.Response.StatusCode;

            // Log the details
            _logger.LogInformation("Request Details: IP: {ClientIp}, ClientId: {ClientId}, HTTP Method: {Method}, Endpoint: {Endpoint}, Response Code: {ResponseCode}, Response Time: {ResponseTime} ms",
                clientIp, clientId, method, endpoint, responseCode, responseTime);

        }
    }

}
