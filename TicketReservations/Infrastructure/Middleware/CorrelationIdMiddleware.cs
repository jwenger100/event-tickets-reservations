using System.Diagnostics;
using TicketReservations.Infrastructure.Extensions;

namespace TicketReservations.Infrastructure.Middleware
{
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CorrelationIdMiddleware> _logger;

        public const string CorrelationIdHeaderName = "X-Correlation-ID";

        public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            
            var correlationId = GetOrGenerateCorrelationId(context);
            context.SetCorrelationId(correlationId);
            context.Response.Headers.TryAdd(CorrelationIdHeaderName, correlationId);
            
            using var scope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId
            });

            await _next(context);
        }

        private static string GetOrGenerateCorrelationId(HttpContext context)
        {
            // Check for incoming header first
            if (context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var correlationIdFromHeader))
            {
                var headerValue = correlationIdFromHeader.FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(headerValue))
                {
                    return headerValue;
                }
            }

            // Generate a clean GUID if no correlation ID provided
            return Guid.NewGuid().ToString();
        }
    }
} 