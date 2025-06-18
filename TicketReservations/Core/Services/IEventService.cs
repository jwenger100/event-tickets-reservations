using TicketReservations.DTOs;
using TicketReservations.Models;

namespace TicketReservations.Core.Services
{
    public interface IEventService
    {
        Task<IEnumerable<EventSummaryDto>> GetAllAsync(EventStatus? status = null);
        Task<EventDto?> GetByIdAsync(int id);
        Task<EventDto> CreateAsync(CreateEventDto createEventDto);
        Task<bool> UpdateAsync(int id, UpdateEventDto updateEventDto);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<EventSummaryDto>> GetOnSaleAsync();
        Task<bool> ExistsAsync(int id);
    }
} 