using TicketReservations.DTOs;

namespace TicketReservations.Core.Services
{
    public interface IVenueService
    {
        Task<IEnumerable<VenueDto>> GetAllAsync();
        Task<VenueDto?> GetByIdAsync(int id);
        Task<VenueDto> CreateAsync(CreateVenueDto createVenueDto);
        Task<bool> UpdateAsync(int id, UpdateVenueDto updateVenueDto);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
} 