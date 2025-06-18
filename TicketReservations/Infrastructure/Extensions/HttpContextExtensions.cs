namespace TicketReservations.Infrastructure.Extensions
{
    public static class HttpContextExtensions
    {
        public const string CorrelationIdKey = "X-Correlation-ID";

        /// <summary>
        /// Gets the correlation ID for the current request
        /// </summary>
        public static string GetCorrelationId(this HttpContext context)
        {
            return context.Items[CorrelationIdKey]?.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Sets the correlation ID for the current request
        /// </summary>
        public static void SetCorrelationId(this HttpContext context, string correlationId)
        {
            context.Items[CorrelationIdKey] = correlationId;
        }
    }
} 