using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TicketReservations.Core.Services;
using TicketReservations.Data;
using TicketReservations.DTOs;
using TicketReservations.Models;

namespace TicketReservations.Infrastructure.Services
{
    public class ReservationService : IReservationService
    {
        private readonly TicketReservationContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ReservationService> _logger;

        public ReservationService(
            TicketReservationContext context,
            IConfiguration configuration,
            ILogger<ReservationService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<ReservationDto> CreateReservationAsync(int eventId, CreateReservationDto createReservationDto)
        {
            // Validate ticket type exists and belongs to the event
            var ticketType = await _context.TicketTypes
                .Include(tt => tt.Event)
                .FirstOrDefaultAsync(tt => tt.Id == createReservationDto.TicketTypeId && tt.EventId == eventId);

            if (ticketType == null)
            {
                throw new ArgumentException("Invalid ticket type for the specified event");
            }

            // Check if event is on sale
            if (ticketType.Event?.Status != EventStatus.OnSale)
            {
                throw new InvalidOperationException("Event tickets are not currently on sale");
            }

            // Check ticket availability
            var availableTickets = await GetAvailableTicketCountAsync(createReservationDto.TicketTypeId);
            if (availableTickets < createReservationDto.Quantity)
            {
                throw new InvalidOperationException($"Only {availableTickets} tickets available");
            }

            // Get reservation timeout from configuration
            var timeoutMinutes = _configuration.GetValue<int>("ReservationSettings:DefaultTimeoutMinutes", 15);

            // Create reservation
            var reservation = new Reservation
            {
                EventId = eventId,
                TicketTypeId = createReservationDto.TicketTypeId,
                CustomerName = createReservationDto.CustomerName,
                CustomerEmail = createReservationDto.CustomerEmail,
                CustomerPhone = createReservationDto.CustomerPhone,
                Quantity = createReservationDto.Quantity,
                UnitPrice = ticketType.Price,
                TotalAmount = ticketType.Price * createReservationDto.Quantity,
                Status = ReservationStatus.Active,
                ReservationDate = DateTime.UtcNow,
                ReservationTimeoutMinutes = timeoutMinutes,
                ExpirationDate = DateTime.UtcNow.AddMinutes(timeoutMinutes),
                ReservationCode = GenerateReservationCode()
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Reservation {ReservationId} created for {Quantity} tickets", 
                reservation.Id, reservation.Quantity);

            return await MapToReservationDto(reservation);
        }

        public async Task<ReservationDto?> GetReservationByCodeAsync(string reservationCode)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Event)
                .Include(r => r.TicketType)
                .FirstOrDefaultAsync(r => r.ReservationCode == reservationCode);

            return reservation != null ? await MapToReservationDto(reservation) : null;
        }



        public async Task<SaleDto> PurchaseReservationAsync(int reservationId, CreateSaleFromReservationDto purchaseDto)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Event)
                .Include(r => r.TicketType)
                .FirstOrDefaultAsync(r => r.Id == reservationId);

            if (reservation == null)
            {
                throw new ArgumentException("Reservation not found");
            }

            if (reservation.Status != ReservationStatus.Active)
            {
                throw new InvalidOperationException("Reservation is not active");
            }

            if (DateTime.UtcNow > reservation.ExpirationDate)
            {
                throw new InvalidOperationException("Reservation has expired");
            }

            // Calculate fees
            var serviceFee = reservation.TotalAmount * 0.05m; // 5% service fee
            var tax = (reservation.TotalAmount + serviceFee) * 0.08m; // 8% tax
            var finalAmount = reservation.TotalAmount + serviceFee + tax;

            // Create sale
            var sale = new Sale
            {
                EventId = reservation.EventId,
                TicketTypeId = reservation.TicketTypeId,
                CustomerName = reservation.CustomerName,
                CustomerEmail = reservation.CustomerEmail,
                CustomerPhone = reservation.CustomerPhone,
                Quantity = reservation.Quantity,
                UnitPrice = reservation.UnitPrice,
                TotalAmount = reservation.TotalAmount,
                ServiceFee = serviceFee,
                Tax = tax,
                FinalAmount = finalAmount,
                Status = SaleStatus.Completed,
                PaymentMethod = purchaseDto.PaymentMethod,
                SaleDate = DateTime.UtcNow,
                PaymentDate = DateTime.UtcNow,
                ConfirmationCode = GenerateConfirmationCode(),
                Notes = purchaseDto.Notes
            };

            _context.Sales.Add(sale);
            await _context.SaveChangesAsync();

            // Update reservation
            reservation.Status = ReservationStatus.ConvertedToSale;
            reservation.ConvertedToSaleDate = DateTime.UtcNow;
            reservation.SaleId = sale.Id;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Reservation {ReservationId} converted to sale {SaleId}", 
                reservationId, sale.Id);

            return await MapToSaleDto(sale);
        }

        public async Task<bool> CancelReservationAsync(int reservationId, string? reason = null)
        {
            var reservation = await _context.Reservations.FindAsync(reservationId);
            
            if (reservation == null)
            {
                return false;
            }

            if (reservation.Status != ReservationStatus.Active)
            {
                return false;
            }

            reservation.Status = ReservationStatus.Cancelled;
            reservation.ReleasedDate = DateTime.UtcNow;
            reservation.ReleasedReason = reason ?? "Cancelled by customer";

            await _context.SaveChangesAsync();

            _logger.LogInformation("Reservation {ReservationId} cancelled", reservationId);

            return true;
        }

        public async Task<bool> ExtendReservationAsync(int reservationId, int additionalMinutes)
        {
            var reservation = await _context.Reservations.FindAsync(reservationId);
            
            if (reservation == null || reservation.Status != ReservationStatus.Active)
            {
                return false;
            }

            if (additionalMinutes <= 0)
            {
                return false;
            }

            reservation.ExpirationDate = reservation.ExpirationDate.AddMinutes(additionalMinutes);
            reservation.ReservationTimeoutMinutes += additionalMinutes;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Reservation {ReservationId} extended by {Minutes} minutes", 
                reservationId, additionalMinutes);

            return true;
        }



        public async Task ReleaseExpiredReservationsAsync()
        {
            var expiredReservations = await _context.Reservations
                .Where(r => r.Status == ReservationStatus.Active && DateTime.UtcNow > r.ExpirationDate)
                .ToListAsync();

            foreach (var reservation in expiredReservations)
            {
                reservation.Status = ReservationStatus.Expired;
                reservation.ReleasedDate = DateTime.UtcNow;
                reservation.ReleasedReason = "Automatic expiration";
            }

            if (expiredReservations.Any())
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Released {Count} expired reservations", expiredReservations.Count);
            }
        }

        public async Task<bool> IsReservationValidAsync(int reservationId)
        {
            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.Id == reservationId);

            return reservation != null && 
                   reservation.Status == ReservationStatus.Active && 
                   DateTime.UtcNow <= reservation.ExpirationDate;
        }

        public async Task<int> GetAvailableTicketCountAsync(int ticketTypeId)
        {
            var ticketType = await _context.TicketTypes.FindAsync(ticketTypeId);
            if (ticketType == null) return 0;

            // Calculate reserved tickets (active reservations)
            var reservedCount = await _context.Reservations
                .Where(r => r.TicketTypeId == ticketTypeId && 
                           r.Status == ReservationStatus.Active && 
                           DateTime.UtcNow <= r.ExpirationDate)
                .SumAsync(r => r.Quantity);

            var soldCount = await _context.Sales
                .Where(s => s.TicketTypeId == ticketTypeId && 
                           (s.Status == SaleStatus.Completed))
                .SumAsync(s => s.Quantity);

            return Math.Max(0, ticketType.TotalQuantity - reservedCount - soldCount);
        }

        private static string GenerateReservationCode()
        {
            return "RES" + Guid.NewGuid().ToString("N")[..8].ToUpper();
        }

        private static string GenerateConfirmationCode()
        {
            return "CONF" + Guid.NewGuid().ToString("N")[..8].ToUpper();
        }

        private async Task<ReservationDto> MapToReservationDto(Reservation reservation)
        {
            if (reservation.Event == null)
            {
                reservation.Event = await _context.Events.FindAsync(reservation.EventId);
            }
            if (reservation.TicketType == null)
            {
                reservation.TicketType = await _context.TicketTypes.FindAsync(reservation.TicketTypeId);
            }

            return new ReservationDto
            {
                Id = reservation.Id,
                EventId = reservation.EventId,
                EventTitle = reservation.Event?.Title ?? "",
                TicketTypeId = reservation.TicketTypeId,
                TicketTypeName = reservation.TicketType?.Name ?? "",
                CustomerName = reservation.CustomerName,
                CustomerEmail = reservation.CustomerEmail,
                CustomerPhone = reservation.CustomerPhone,
                Quantity = reservation.Quantity,
                UnitPrice = reservation.UnitPrice,
                TotalAmount = reservation.TotalAmount,
                Status = reservation.Status,
                ReservationDate = reservation.ReservationDate,
                ExpirationDate = reservation.ExpirationDate,
                ReservationTimeoutMinutes = reservation.ReservationTimeoutMinutes,
                ReservationCode = reservation.ReservationCode,
                SaleId = reservation.SaleId
            };
        }

        private async Task<SaleDto> MapToSaleDto(Sale sale)
        {
            if (sale.Event == null)
            {
                sale.Event = await _context.Events.FindAsync(sale.EventId);
            }
            if (sale.TicketType == null)
            {
                sale.TicketType = await _context.TicketTypes.FindAsync(sale.TicketTypeId);
            }

            return new SaleDto
            {
                Id = sale.Id,
                EventId = sale.EventId,
                EventTitle = sale.Event?.Title ?? "",
                TicketTypeId = sale.TicketTypeId,
                TicketTypeName = sale.TicketType?.Name ?? "",
                CustomerName = sale.CustomerName,
                CustomerEmail = sale.CustomerEmail,
                CustomerPhone = sale.CustomerPhone,
                Quantity = sale.Quantity,
                UnitPrice = sale.UnitPrice,
                TotalAmount = sale.TotalAmount,
                ServiceFee = sale.ServiceFee,
                Tax = sale.Tax,
                FinalAmount = sale.FinalAmount,
                Status = sale.Status,
                PaymentTransactionId = sale.PaymentTransactionId,
                PaymentMethod = sale.PaymentMethod,
                SaleDate = sale.SaleDate,
                PaymentDate = sale.PaymentDate,
                ConfirmationCode = sale.ConfirmationCode,
                Notes = sale.Notes
            };
        }
    }
} 