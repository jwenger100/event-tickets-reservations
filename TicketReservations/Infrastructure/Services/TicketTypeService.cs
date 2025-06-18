using Microsoft.EntityFrameworkCore;
using TicketReservations.Core.Services;
using TicketReservations.Data;
using TicketReservations.DTOs;
using TicketReservations.Models;

namespace TicketReservations.Infrastructure.Services
{
    public class TicketTypeService : ITicketTypeService
    {
        private readonly TicketReservationContext _context;
        private readonly IReservationService _reservationService;
        private readonly ILogger<TicketTypeService> _logger;

        public TicketTypeService(TicketReservationContext context, IReservationService reservationService, ILogger<TicketTypeService> logger)
        {
            _context = context;
            _reservationService = reservationService;
            _logger = logger;
        }

        public async Task<IEnumerable<TicketTypeDto>> GetByEventIdAsync(int eventId, bool availableOnly = false)
        {
            var eventEntity = await _context.Events
                .Include(e => e.TicketTypes)
                .FirstOrDefaultAsync(e => e.Id == eventId);
                
            if (eventEntity == null)
            {
                throw new ArgumentException($"Event with ID {eventId} not found.");
            }

            var ticketTypes = eventEntity.TicketTypes.AsQueryable();

            var ticketTypesList = ticketTypes.ToList();
            var result = new List<TicketTypeDto>();

            foreach (var tt in ticketTypesList)
            {
                // Get real-time counts
                var realTimeAvailableCount = await _reservationService.GetAvailableTicketCountAsync(tt.Id);
                var realTimeReservedCount = await GetReservedTicketCountAsync(tt.Id);
                var realTimeSoldCount = await GetSoldTicketCountAsync(tt.Id);
                
                var isCurrentlyAvailable = tt.IsActive && realTimeAvailableCount > 0 && 
                                         (tt.SaleStartDate == null || tt.SaleStartDate <= DateTime.UtcNow) &&
                                         (tt.SaleEndDate == null || tt.SaleEndDate >= DateTime.UtcNow) &&
                                         eventEntity.EventDate > DateTime.UtcNow;

                // Skip if filtering for available only and this ticket type is not available
                if (availableOnly && !isCurrentlyAvailable)
                {
                    continue;
                }

                var ticketTypeDto = new TicketTypeDto
                {
                    Id = tt.Id,
                    EventId = tt.EventId,
                    EventTitle = eventEntity.Title,
                    Name = tt.Name,
                    Description = tt.Description,
                    Price = tt.Price,
                    TotalQuantity = tt.TotalQuantity,
                    AvailableQuantity = realTimeAvailableCount, // Use real-time count
                    ReservedQuantity = realTimeReservedCount, // Use real-time count
                    SoldQuantity = realTimeSoldCount, // Use real-time count
                    SaleStartDate = tt.SaleStartDate,
                    SaleEndDate = tt.SaleEndDate,
                    MaxPerCustomer = tt.MaxPerCustomer,
                    IsActive = tt.IsActive,
                    Category = tt.Category,
                    IsAvailable = isCurrentlyAvailable,
                    UnavailableReason = GetUnavailableReasonWithRealTimeCount(tt, eventEntity, realTimeAvailableCount)
                };

                result.Add(ticketTypeDto);
            }

            return result;
        }

        public async Task<TicketTypeDto?> GetByIdAsync(int eventId, int ticketTypeId)
        {
            var ticketType = await _context.TicketTypes
                .Include(tt => tt.Event)
                .FirstOrDefaultAsync(tt => tt.Id == ticketTypeId && tt.EventId == eventId);

            if (ticketType == null)
            {
                return null;
            }

            // Get real-time available count
            var realTimeAvailableCount = await _reservationService.GetAvailableTicketCountAsync(ticketType.Id);

            var isCurrentlyAvailable = ticketType.IsActive && realTimeAvailableCount > 0 && 
                                     (ticketType.SaleStartDate == null || ticketType.SaleStartDate <= DateTime.UtcNow) &&
                                     (ticketType.SaleEndDate == null || ticketType.SaleEndDate >= DateTime.UtcNow) &&
                                     ticketType.Event!.EventDate > DateTime.UtcNow;

            return new TicketTypeDto
            {
                Id = ticketType.Id,
                EventId = ticketType.EventId,
                EventTitle = ticketType.Event!.Title,
                Name = ticketType.Name,
                Description = ticketType.Description,
                Price = ticketType.Price,
                TotalQuantity = ticketType.TotalQuantity,
                AvailableQuantity = realTimeAvailableCount, // Use real-time count
                ReservedQuantity = await GetReservedTicketCountAsync(ticketType.Id), // Use real-time count
                SoldQuantity = await GetSoldTicketCountAsync(ticketType.Id), // Use real-time count
                SaleStartDate = ticketType.SaleStartDate,
                SaleEndDate = ticketType.SaleEndDate,
                MaxPerCustomer = ticketType.MaxPerCustomer,
                IsActive = ticketType.IsActive,
                Category = ticketType.Category,
                IsAvailable = isCurrentlyAvailable,
                UnavailableReason = GetUnavailableReasonWithRealTimeCount(ticketType, ticketType.Event!, realTimeAvailableCount)
            };
        }

        public async Task<TicketTypeDto> CreateAsync(int eventId, CreateTicketTypeDto createTicketTypeDto)
        {
            // Business rule: Validate event exists
            var eventEntity = await _context.Events.FindAsync(eventId);
            if (eventEntity == null)
            {
                throw new ArgumentException($"Event with ID {eventId} not found.");
            }

            // Business rule: Validate event ID consistency
            if (createTicketTypeDto.EventId != eventId)
            {
                throw new ArgumentException("Event ID in route must match Event ID in request body.");
            }

            // Business rule: Validate quantity
            if (createTicketTypeDto.TotalQuantity <= 0)
            {
                throw new ArgumentException("Total quantity must be greater than zero.");
            }

            // Business rule: Validate price
            if (createTicketTypeDto.Price < 0)
            {
                throw new ArgumentException("Price cannot be negative.");
            }

            var ticketType = new TicketType
            {
                EventId = eventId,
                Name = createTicketTypeDto.Name,
                Description = createTicketTypeDto.Description,
                Price = createTicketTypeDto.Price,
                TotalQuantity = createTicketTypeDto.TotalQuantity,
                AvailableQuantity = createTicketTypeDto.TotalQuantity,
                ReservedQuantity = 0,
                SoldQuantity = 0,
                SaleStartDate = createTicketTypeDto.SaleStartDate,
                SaleEndDate = createTicketTypeDto.SaleEndDate,
                MaxPerCustomer = createTicketTypeDto.MaxPerCustomer,
                Category = createTicketTypeDto.Category,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.TicketTypes.Add(ticketType);
            await _context.SaveChangesAsync();

            // Get real-time available count (for new ticket type, this should be the same as AvailableQuantity)
            var realTimeAvailableCount = await _reservationService.GetAvailableTicketCountAsync(ticketType.Id);

            var isCurrentlyAvailable = ticketType.IsActive && realTimeAvailableCount > 0 && 
                                     (ticketType.SaleStartDate == null || ticketType.SaleStartDate <= DateTime.UtcNow) &&
                                     (ticketType.SaleEndDate == null || ticketType.SaleEndDate >= DateTime.UtcNow) &&
                                     eventEntity.EventDate > DateTime.UtcNow;

            return new TicketTypeDto
            {
                Id = ticketType.Id,
                EventId = ticketType.EventId,
                EventTitle = eventEntity.Title,
                Name = ticketType.Name,
                Description = ticketType.Description,
                Price = ticketType.Price,
                TotalQuantity = ticketType.TotalQuantity,
                AvailableQuantity = realTimeAvailableCount, // Use real-time count
                ReservedQuantity = await GetReservedTicketCountAsync(ticketType.Id), // Use real-time count
                SoldQuantity = await GetSoldTicketCountAsync(ticketType.Id), // Use real-time count
                SaleStartDate = ticketType.SaleStartDate,
                SaleEndDate = ticketType.SaleEndDate,
                MaxPerCustomer = ticketType.MaxPerCustomer,
                IsActive = ticketType.IsActive,
                Category = ticketType.Category,
                IsAvailable = isCurrentlyAvailable,
                UnavailableReason = GetUnavailableReasonWithRealTimeCount(ticketType, eventEntity, realTimeAvailableCount)
            };
        }

        public async Task<bool> UpdateAsync(int eventId, int ticketTypeId, UpdateTicketTypeDto updateTicketTypeDto)
        {
            var ticketType = await _context.TicketTypes
                .FirstOrDefaultAsync(tt => tt.Id == ticketTypeId && tt.EventId == eventId);

            if (ticketType == null)
            {
                return false;
            }

            // Business rule: Validate price if provided
            if (updateTicketTypeDto.Price.HasValue && updateTicketTypeDto.Price.Value < 0)
            {
                throw new ArgumentException("Price cannot be negative.");
            }

            // Business rule: Validate quantity if provided
            if (updateTicketTypeDto.TotalQuantity.HasValue && updateTicketTypeDto.TotalQuantity.Value <= 0)
            {
                throw new ArgumentException("Total quantity must be greater than zero.");
            }

            // Update only provided fields
            if (!string.IsNullOrEmpty(updateTicketTypeDto.Name))
                ticketType.Name = updateTicketTypeDto.Name;

            if (updateTicketTypeDto.Description != null)
                ticketType.Description = updateTicketTypeDto.Description;

            if (updateTicketTypeDto.Price.HasValue)
                ticketType.Price = updateTicketTypeDto.Price.Value;

            if (updateTicketTypeDto.TotalQuantity.HasValue)
            {
                var difference = updateTicketTypeDto.TotalQuantity.Value - ticketType.TotalQuantity;
                ticketType.TotalQuantity = updateTicketTypeDto.TotalQuantity.Value;
                ticketType.AvailableQuantity += difference; // Adjust available quantity accordingly
            }

            if (updateTicketTypeDto.SaleStartDate.HasValue)
                ticketType.SaleStartDate = updateTicketTypeDto.SaleStartDate.Value;

            if (updateTicketTypeDto.SaleEndDate.HasValue)
                ticketType.SaleEndDate = updateTicketTypeDto.SaleEndDate.Value;

            if (updateTicketTypeDto.MaxPerCustomer.HasValue)
                ticketType.MaxPerCustomer = updateTicketTypeDto.MaxPerCustomer.Value;

            if (updateTicketTypeDto.IsActive.HasValue)
                ticketType.IsActive = updateTicketTypeDto.IsActive.Value;

            if (updateTicketTypeDto.Category.HasValue)
                ticketType.Category = updateTicketTypeDto.Category.Value;

            ticketType.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ExistsAsync(ticketTypeId))
                {
                    return false;
                }
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int eventId, int ticketTypeId)
        {
            var ticketType = await _context.TicketTypes
                .FirstOrDefaultAsync(tt => tt.Id == ticketTypeId && tt.EventId == eventId);

            if (ticketType == null)
            {
                return false;
            }

            // Check if there are any active reservations or sales
            var hasActiveReservations = await _context.Reservations
                .AnyAsync(r => r.TicketTypeId == ticketTypeId && r.Status == ReservationStatus.Active);

            var hasCompletedSales = await _context.Sales
                .AnyAsync(s => s.TicketTypeId == ticketTypeId && s.Status == SaleStatus.Completed);

            if (hasActiveReservations || hasCompletedSales)
            {
                throw new InvalidOperationException("Cannot delete ticket type with active reservations or completed sales.");
            }

            _context.TicketTypes.Remove(ticketType);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.TicketTypes.AnyAsync(tt => tt.Id == id);
        }

        private static string? GetUnavailableReason(TicketType ticketType, Event eventEntity)
        {
            if (!ticketType.IsActive)
                return "Ticket type is not active";

            if (ticketType.AvailableQuantity <= 0)
                return "Sold out";

            if (ticketType.SaleStartDate.HasValue && ticketType.SaleStartDate > DateTime.UtcNow)
                return $"Sales start on {ticketType.SaleStartDate:MM/dd/yyyy}";

            if (ticketType.SaleEndDate.HasValue && ticketType.SaleEndDate < DateTime.UtcNow)
                return "Sales have ended";

            if (eventEntity.EventDate <= DateTime.UtcNow)
                return "Event has already occurred";

            return null;
        }

        private static string? GetUnavailableReasonWithRealTimeCount(TicketType ticketType, Event eventEntity, int realTimeAvailableCount)
        {
            if (!ticketType.IsActive)
                return "Ticket type is not active";

            if (realTimeAvailableCount <= 0)
                return "Sold out";

            if (ticketType.SaleStartDate.HasValue && ticketType.SaleStartDate > DateTime.UtcNow)
                return $"Sales start on {ticketType.SaleStartDate:MM/dd/yyyy}";

            if (ticketType.SaleEndDate.HasValue && ticketType.SaleEndDate < DateTime.UtcNow)
                return "Sales have ended";

            if (eventEntity.EventDate <= DateTime.UtcNow)
                return "Event has already occurred";

            return null;
        }

        private async Task<int> GetReservedTicketCountAsync(int ticketTypeId)
        {
            // Calculate reserved tickets (active, non-expired reservations)
            return await _context.Reservations
                .Where(r => r.TicketTypeId == ticketTypeId && 
                           r.Status == ReservationStatus.Active && 
                           DateTime.UtcNow <= r.ExpirationDate)
                .SumAsync(r => r.Quantity);
        }

        private async Task<int> GetSoldTicketCountAsync(int ticketTypeId)
        {
            // Calculate sold tickets (completed sales)
            return await _context.Sales
                .Where(s => s.TicketTypeId == ticketTypeId && 
                           (s.Status == SaleStatus.Completed))
                .SumAsync(s => s.Quantity);
        }
    }
} 