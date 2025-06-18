using TicketReservations.DTOs;
using TicketReservations.Models;

namespace TicketReservations.Core.Services
{
    public interface IReservationService
    {
        Task<ReservationDto> CreateReservationAsync(int eventId, CreateReservationDto createReservationDto);
        Task<ReservationDto?> GetReservationByCodeAsync(string reservationCode);
        Task<SaleDto> PurchaseReservationAsync(int reservationId, CreateSaleFromReservationDto purchaseDto);
        Task<bool> CancelReservationAsync(int reservationId, string? reason = null);
        Task<bool> ExtendReservationAsync(int reservationId, int additionalMinutes);
        Task ReleaseExpiredReservationsAsync();
        Task<bool> IsReservationValidAsync(int reservationId);
        Task<int> GetAvailableTicketCountAsync(int ticketTypeId);
    }
} 