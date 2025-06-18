using Microsoft.EntityFrameworkCore;
using TicketReservations.Core.Services;
using TicketReservations.Data;
using TicketReservations.DTOs;
using TicketReservations.Models;

namespace TicketReservations.Infrastructure.Services
{
    public class EventService : IEventService
    {
        private readonly TicketReservationContext _context;
        private readonly IReservationService _reservationService;
        private readonly ILogger<EventService> _logger;

        public EventService(TicketReservationContext context, IReservationService reservationService, ILogger<EventService> logger)
        {
            _context = context;
            _reservationService = reservationService;
            _logger = logger;
        }

        public async Task<IEnumerable<EventSummaryDto>> GetAllAsync(EventStatus? status = null)
        {
            var query = _context.Events.Include(e => e.Venue).Include(e => e.TicketTypes).AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(e => e.Status == status.Value);
            }

            var events = await query
                .OrderBy(e => e.EventDate)
                .ToListAsync();

            var eventSummaries = new List<EventSummaryDto>();

            foreach (var eventEntity in events)
            {
                var totalAvailableTickets = 0;
                foreach (var ticketType in eventEntity.TicketTypes)
                {
                    var realTimeCount = await _reservationService.GetAvailableTicketCountAsync(ticketType.Id);
                    totalAvailableTickets += realTimeCount;
                }

                eventSummaries.Add(new EventSummaryDto
                {
                    Id = eventEntity.Id,
                    Title = eventEntity.Title,
                    EventDate = eventEntity.EventDate,
                    VenueName = eventEntity.Venue!.Name,
                    Artist = eventEntity.Artist,
                    Status = eventEntity.Status,
                    TotalTicketTypes = eventEntity.TicketTypes.Count,
                    LowestPrice = eventEntity.TicketTypes.Any() ? eventEntity.TicketTypes.Min(tt => tt.Price) : null,
                    TotalAvailableTickets = totalAvailableTickets // Use real-time count
                });
            }

            return eventSummaries;
        }

        public async Task<EventDto?> GetByIdAsync(int id)
        {
            var eventEntity = await _context.Events
                .Include(e => e.Venue)
                .Include(e => e.TicketTypes)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (eventEntity == null)
            {
                return null;
            }

            return new EventDto
            {
                Id = eventEntity.Id,
                Title = eventEntity.Title,
                Description = eventEntity.Description,
                EventDate = eventEntity.EventDate,
                DoorsOpen = eventEntity.DoorsOpen,
                VenueId = eventEntity.VenueId,
                Venue = eventEntity.Venue != null ? new VenueDto
                {
                    Id = eventEntity.Venue.Id,
                    Name = eventEntity.Venue.Name,
                    Address = eventEntity.Venue.Address,
                    City = eventEntity.Venue.City,
                    State = eventEntity.Venue.State,
                    ZipCode = eventEntity.Venue.ZipCode,
                    TotalCapacity = eventEntity.Venue.TotalCapacity,
                    Description = eventEntity.Venue.Description
                } : null,
                Status = eventEntity.Status,
                Artist = eventEntity.Artist,
                Genre = eventEntity.Genre,
                MinimumAge = eventEntity.MinimumAge,
                ImageUrl = eventEntity.ImageUrl,
                TicketTypes = await GetTicketTypesWithRealTimeCountsAsync(eventEntity)
            };
        }

        public async Task<EventDto> CreateAsync(CreateEventDto createEventDto)
        {
            // Business rule: Validate venue exists
            var venue = await _context.Venues.FindAsync(createEventDto.VenueId);
            if (venue == null)
            {
                throw new ArgumentException($"Venue with ID {createEventDto.VenueId} not found.");
            }

            // Business rule: Validate event date
            if (createEventDto.EventDate <= DateTime.UtcNow)
            {
                throw new ArgumentException("Event date must be in the future.");
            }

            var eventEntity = new Event
            {
                Title = createEventDto.Title,
                Description = createEventDto.Description,
                EventDate = createEventDto.EventDate,
                DoorsOpen = createEventDto.DoorsOpen,
                VenueId = createEventDto.VenueId,
                Status = EventStatus.Scheduled,
                Artist = createEventDto.Artist,
                Genre = createEventDto.Genre,
                MinimumAge = createEventDto.MinimumAge,
                ImageUrl = createEventDto.ImageUrl,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Events.Add(eventEntity);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(eventEntity.Id) ?? throw new InvalidOperationException("Failed to retrieve created event.");
        }

        public async Task<bool> UpdateAsync(int id, UpdateEventDto updateEventDto)
        {
            var eventEntity = await _context.Events.FindAsync(id);

            if (eventEntity == null)
            {
                return false;
            }

            // Business rule: Validate event date if provided
            if (updateEventDto.EventDate.HasValue && updateEventDto.EventDate.Value <= DateTime.UtcNow)
            {
                throw new ArgumentException("Event date must be in the future.");
            }

            // Business rule: Validate venue exists if provided
            if (updateEventDto.VenueId.HasValue)
            {
                var venue = await _context.Venues.FindAsync(updateEventDto.VenueId.Value);
                if (venue == null)
                {
                    throw new ArgumentException($"Venue with ID {updateEventDto.VenueId.Value} not found.");
                }
            }

            // Update only provided fields
            if (!string.IsNullOrEmpty(updateEventDto.Title))
                eventEntity.Title = updateEventDto.Title;

            if (!string.IsNullOrEmpty(updateEventDto.Description))
                eventEntity.Description = updateEventDto.Description;

            if (updateEventDto.EventDate.HasValue)
                eventEntity.EventDate = updateEventDto.EventDate.Value;

            if (updateEventDto.DoorsOpen.HasValue)
                eventEntity.DoorsOpen = updateEventDto.DoorsOpen.Value;

            if (updateEventDto.VenueId.HasValue)
                eventEntity.VenueId = updateEventDto.VenueId.Value;

            if (updateEventDto.Status.HasValue)
                eventEntity.Status = updateEventDto.Status.Value;

            if (!string.IsNullOrEmpty(updateEventDto.Artist))
                eventEntity.Artist = updateEventDto.Artist;

            if (!string.IsNullOrEmpty(updateEventDto.Genre))
                eventEntity.Genre = updateEventDto.Genre;

            if (updateEventDto.MinimumAge.HasValue)
                eventEntity.MinimumAge = updateEventDto.MinimumAge.Value;

            if (!string.IsNullOrEmpty(updateEventDto.ImageUrl))
                eventEntity.ImageUrl = updateEventDto.ImageUrl;

            eventEntity.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ExistsAsync(id))
                {
                    return false;
                }
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var eventEntity = await _context.Events.FindAsync(id);

            if (eventEntity == null)
            {
                return false;
            }

            // Business rule: Check if there are any active reservations or sales
            var hasActiveReservations = await _context.Reservations
                .AnyAsync(r => r.EventId == id && r.Status == ReservationStatus.Active);

            var hasCompletedSales = await _context.Sales
                .AnyAsync(s => s.EventId == id && s.Status == SaleStatus.Completed);

            if (hasActiveReservations || hasCompletedSales)
            {
                throw new InvalidOperationException("Cannot delete event with active reservations or completed sales.");
            }

            _context.Events.Remove(eventEntity);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<EventSummaryDto>> GetOnSaleAsync()
        {
            var events = await _context.Events
                .Include(e => e.Venue)
                .Include(e => e.TicketTypes)
                .Where(e => e.Status == EventStatus.OnSale && 
                           e.EventDate > DateTime.UtcNow &&
                           e.TicketTypes.Any(tt => tt.IsActive))
                .OrderBy(e => e.EventDate)
                .ToListAsync();

            var eventSummaries = new List<EventSummaryDto>();

            foreach (var eventEntity in events)
            {
                var totalAvailableTickets = 0;
                var hasAvailableTickets = false;

                foreach (var ticketType in eventEntity.TicketTypes.Where(tt => tt.IsActive))
                {
                    var realTimeCount = await _reservationService.GetAvailableTicketCountAsync(ticketType.Id);
                    totalAvailableTickets += realTimeCount;
                    if (realTimeCount > 0)
                    {
                        hasAvailableTickets = true;
                    }
                }

                // Only include events that have available tickets
                if (hasAvailableTickets)
                {
                    eventSummaries.Add(new EventSummaryDto
                    {
                        Id = eventEntity.Id,
                        Title = eventEntity.Title,
                        EventDate = eventEntity.EventDate,
                        VenueName = eventEntity.Venue!.Name,
                        Artist = eventEntity.Artist,
                        Status = eventEntity.Status,
                        TotalTicketTypes = eventEntity.TicketTypes.Count,
                        LowestPrice = eventEntity.TicketTypes.Where(tt => tt.IsActive).Min(tt => tt.Price),
                        TotalAvailableTickets = totalAvailableTickets // Use real-time count
                    });
                }
            }

            return eventSummaries;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Events.AnyAsync(e => e.Id == id);
        }

        private async Task<List<TicketTypeDto>> GetTicketTypesWithRealTimeCountsAsync(Event eventEntity)
        {
            var ticketTypeDtos = new List<TicketTypeDto>();

            foreach (var tt in eventEntity.TicketTypes)
            {
                var realTimeAvailableCount = await _reservationService.GetAvailableTicketCountAsync(tt.Id);

                var isCurrentlyAvailable = tt.IsActive && realTimeAvailableCount > 0 && 
                                         (tt.SaleStartDate == null || tt.SaleStartDate <= DateTime.UtcNow) &&
                                         (tt.SaleEndDate == null || tt.SaleEndDate >= DateTime.UtcNow) &&
                                         eventEntity.EventDate > DateTime.UtcNow;

                ticketTypeDtos.Add(new TicketTypeDto
                {
                    Id = tt.Id,
                    EventId = tt.EventId,
                    EventTitle = eventEntity.Title,
                    Name = tt.Name,
                    Description = tt.Description,
                    Price = tt.Price,
                    TotalQuantity = tt.TotalQuantity,
                    AvailableQuantity = realTimeAvailableCount, // Use real-time count
                    ReservedQuantity = tt.ReservedQuantity,
                    SoldQuantity = tt.SoldQuantity,
                    SaleStartDate = tt.SaleStartDate,
                    SaleEndDate = tt.SaleEndDate,
                    MaxPerCustomer = tt.MaxPerCustomer,
                    IsActive = tt.IsActive,
                    Category = tt.Category,
                    IsAvailable = isCurrentlyAvailable,
                    UnavailableReason = GetUnavailableReasonWithRealTimeCount(tt, eventEntity, realTimeAvailableCount)
                });
            }

            return ticketTypeDtos;
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
    }
} 