using Microsoft.EntityFrameworkCore;
using TicketReservations.Core.Services;
using TicketReservations.Data;
using TicketReservations.DTOs;
using TicketReservations.Models;

namespace TicketReservations.Infrastructure.Services
{
    public class VenueService : IVenueService
    {
        private readonly TicketReservationContext _context;
        private readonly ILogger<VenueService> _logger;

        public VenueService(TicketReservationContext context, ILogger<VenueService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<VenueDto>> GetAllAsync()
        {
            var venues = await _context.Venues
                .Select(v => new VenueDto
                {
                    Id = v.Id,
                    Name = v.Name,
                    Address = v.Address,
                    City = v.City,
                    State = v.State,
                    ZipCode = v.ZipCode,
                    TotalCapacity = v.TotalCapacity,
                    Description = v.Description
                })
                .OrderBy(v => v.Name)
                .ToListAsync();

            return venues;
        }

        public async Task<VenueDto?> GetByIdAsync(int id)
        {
            var venue = await _context.Venues.FindAsync(id);

            if (venue == null)
            {
                return null;
            }

            return new VenueDto
            {
                Id = venue.Id,
                Name = venue.Name,
                Address = venue.Address,
                City = venue.City,
                State = venue.State,
                ZipCode = venue.ZipCode,
                TotalCapacity = venue.TotalCapacity,
                Description = venue.Description
            };
        }

        public async Task<VenueDto> CreateAsync(CreateVenueDto createVenueDto)
        {
            if (createVenueDto.TotalCapacity <= 0)
            {
                throw new ArgumentException("Total capacity must be greater than zero.");
            }

            var venue = new Venue
            {
                Name = createVenueDto.Name,
                Address = createVenueDto.Address,
                City = createVenueDto.City,
                State = createVenueDto.State,
                ZipCode = createVenueDto.ZipCode,
                TotalCapacity = createVenueDto.TotalCapacity,
                Description = createVenueDto.Description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Venues.Add(venue);
            await _context.SaveChangesAsync();

            return new VenueDto
            {
                Id = venue.Id,
                Name = venue.Name,
                Address = venue.Address,
                City = venue.City,
                State = venue.State,
                ZipCode = venue.ZipCode,
                TotalCapacity = venue.TotalCapacity,
                Description = venue.Description
            };
        }

        public async Task<bool> UpdateAsync(int id, UpdateVenueDto updateVenueDto)
        {
            var venue = await _context.Venues.FindAsync(id);

            if (venue == null)
            {
                return false;
            }

            // Business rule: Validate capacity if provided
            if (updateVenueDto.TotalCapacity.HasValue && updateVenueDto.TotalCapacity.Value <= 0)
            {
                throw new ArgumentException("Total capacity must be greater than zero.");
            }

            // Update only provided fields
            if (!string.IsNullOrEmpty(updateVenueDto.Name))
                venue.Name = updateVenueDto.Name;

            if (!string.IsNullOrEmpty(updateVenueDto.Address))
                venue.Address = updateVenueDto.Address;

            if (!string.IsNullOrEmpty(updateVenueDto.City))
                venue.City = updateVenueDto.City;

            if (!string.IsNullOrEmpty(updateVenueDto.State))
                venue.State = updateVenueDto.State;

            if (!string.IsNullOrEmpty(updateVenueDto.ZipCode))
                venue.ZipCode = updateVenueDto.ZipCode;

            if (updateVenueDto.TotalCapacity.HasValue)
                venue.TotalCapacity = updateVenueDto.TotalCapacity.Value;

            if (updateVenueDto.Description != null)
                venue.Description = updateVenueDto.Description;

            venue.UpdatedAt = DateTime.UtcNow;

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
            var venue = await _context.Venues.FindAsync(id);

            if (venue == null)
            {
                return false;
            }

            // Business rule: Check if there are any events at this venue
            var hasEvents = await _context.Events.AnyAsync(e => e.VenueId == id);

            if (hasEvents)
            {
                throw new InvalidOperationException("Cannot delete venue with existing events.");
            }

            _context.Venues.Remove(venue);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Venues.AnyAsync(v => v.Id == id);
        }
    }
} 