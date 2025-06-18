using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using TicketReservations.Core.Services;
using TicketReservations.DTOs;

namespace TicketReservations.Controllers
{
    [ApiController]
    [Route("api/events/{eventId}/reservations")]
    public class ReservationsController : ControllerBase
    {
        private readonly IReservationService _reservationService;
        private readonly ILogger<ReservationsController> _logger;

        public ReservationsController(
            IReservationService reservationService,
            ILogger<ReservationsController> logger)
        {
            _reservationService = reservationService;
            _logger = logger;
        }

        /// <summary>
        /// Create a new ticket reservation
        /// </summary>
        /// <param name="eventId">The event ID</param>
        /// <param name="createReservationDto">Reservation details</param>
        /// <returns>Created reservation</returns>
        [HttpPost]
        public async Task<ActionResult<ReservationDto>> CreateReservation(
            int eventId,
            [FromBody] CreateReservationDto createReservationDto)
        {
            try
            {
                var reservation = await _reservationService.CreateReservationAsync(eventId, createReservationDto);
                
                _logger.LogInformation("Reservation created: {ReservationId} for event {EventId}", 
                    reservation.Id, eventId);

                return CreatedAtAction(
                    nameof(GetReservation),
                    new { eventId, reservationCode = reservation.ReservationCode },
                    reservation);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get a specific reservation by reservation code
        /// </summary>
        /// <param name="eventId">The event ID</param>
        /// <param name="reservationCode">The reservation code</param>
        /// <returns>Reservation details</returns>
        [HttpGet("{reservationCode}")]
        public async Task<ActionResult<ReservationDto>> GetReservation(int eventId, string reservationCode)
        {
            var reservation = await _reservationService.GetReservationByCodeAsync(reservationCode);
            
            if (reservation == null || reservation.EventId != eventId)
            {
                return NotFound(new { message = "Reservation not found" });
            }

            return Ok(reservation);
        }

        /// <summary>
        /// Purchase a reservation (convert to sale)
        /// </summary>
        /// <param name="eventId">The event ID</param>
        /// <param name="reservationCode">The reservation code</param>
        /// <param name="purchaseDto">Purchase details</param>
        /// <returns>Sale details</returns>
        [HttpPost("{reservationCode}/purchase")]
        public async Task<ActionResult<SaleDto>> PurchaseReservation(
            int eventId,
            string reservationCode,
            [FromBody] CreateSaleFromReservationDto purchaseDto)
        {
            try
            {
                // First get the reservation to find the ID
                var reservation = await _reservationService.GetReservationByCodeAsync(reservationCode);
                
                if (reservation == null || reservation.EventId != eventId)
                {
                    return NotFound(new { message = "Reservation not found" });
                }

                var sale = await _reservationService.PurchaseReservationAsync(reservation.Id, purchaseDto);
                
                _logger.LogInformation("Reservation {ReservationCode} (ID: {ReservationId}) purchased, created sale {SaleId}", 
                    reservationCode, reservation.Id, sale.Id);

                return Ok(sale);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Cancel a reservation
        /// </summary>
        /// <param name="eventId">The event ID</param>
        /// <param name="reservationCode">The reservation code</param>
        /// <param name="cancelDto">Cancellation details</param>
        /// <returns>Success status</returns>
        [HttpPost("{reservationCode}/cancel")]
        public async Task<ActionResult> CancelReservation(
            int eventId,
            string reservationCode)
        {
            // First get the reservation to find the ID
            var reservation = await _reservationService.GetReservationByCodeAsync(reservationCode);
            
            if (reservation == null || reservation.EventId != eventId)
            {
                return NotFound(new { message = "Reservation not found" });
            }

            var success = await _reservationService.CancelReservationAsync(
                reservation.Id);

            if (!success)
            {
                return NotFound(new { message = "Reservation not found or cannot be cancelled" });
            }

            _logger.LogInformation("Reservation {ReservationCode} (ID: {ReservationId}) cancelled", 
                reservationCode, reservation.Id);
            
            return Ok(new { message = "Reservation cancelled successfully" });
        }

        /// <summary>
        /// Check if reservation is still valid
        /// </summary>
        /// <param name="eventId">The event ID</param>
        /// <param name="reservationCode">The reservation code</param>
        /// <returns>Validation status</returns>
        [HttpGet("{reservationCode}/status")]
        public async Task<ActionResult> CheckReservationStatus(int eventId, string reservationCode)
        {
            var reservation = await _reservationService.GetReservationByCodeAsync(reservationCode);

            if (reservation == null || reservation.EventId != eventId)
            {
                return NotFound(new { message = "Reservation not found" });
            }

            var isValid = await _reservationService.IsReservationValidAsync(reservation.Id);

            return Ok(new
            {
                reservationCode,
                reservationId = reservation.Id,
                isValid,
                status = reservation.Status.ToString(),
                expirationDate = reservation.ExpirationDate,
                isExpired = reservation.IsExpired
            });
        }
    }
} 