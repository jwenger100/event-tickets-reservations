using TicketReservations.DTOs;

namespace TicketReservations.Core.Services
{
    public interface ITicketTypeService
    {
        Task<IEnumerable<TicketTypeDto>> GetByEventIdAsync(int eventId, bool availableOnly = false);
        Task<TicketTypeDto?> GetByIdAsync(int eventId, int ticketTypeId);
        Task<TicketTypeDto> CreateAsync(int eventId, CreateTicketTypeDto createTicketTypeDto);
        Task<bool> UpdateAsync(int eventId, int ticketTypeId, UpdateTicketTypeDto updateTicketTypeDto);
        Task<bool> DeleteAsync(int eventId, int ticketTypeId);
        Task<bool> ExistsAsync(int id);
    }
} 