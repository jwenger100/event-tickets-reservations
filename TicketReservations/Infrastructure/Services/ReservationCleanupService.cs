using TicketReservations.Core.Services;

namespace TicketReservations.Infrastructure.Services
{
    public class ReservationCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ReservationCleanupService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(5); // Run every 5 minutes

        public ReservationCleanupService(
            IServiceProvider serviceProvider,
            ILogger<ReservationCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Reservation cleanup service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var reservationService = scope.ServiceProvider.GetRequiredService<IReservationService>();
                    
                    await reservationService.ReleaseExpiredReservationsAsync();
                    
                    _logger.LogDebug("Reservation cleanup completed at {Time}", DateTime.UtcNow);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during reservation cleanup");
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }
    }
} 