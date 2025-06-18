using Microsoft.AspNetCore.Mvc;
using TicketReservations.Core.Services;
using TicketReservations.DTOs;

namespace TicketReservations.Controllers
{
    [ApiController]
    [ApiVersion("1")]
    [Route("api/v{version:apiVersion}/events/{eventId}/ticket-types")]
    public class TicketTypesController : ControllerBase
    {
        private readonly ITicketTypeService _ticketTypeService;
        private readonly ILogger<TicketTypesController> _logger;

        public TicketTypesController(ITicketTypeService ticketTypeService, ILogger<TicketTypesController> logger)
        {
            _ticketTypeService = ticketTypeService;
            _logger = logger;
        }

        /// <summary>
        /// Get all ticket types for an event (includes availability information)
        /// </summary>
        /// <param name="eventId">The event ID</param>
        /// <param name="availableOnly">If true, only returns ticket types that are currently available for purchase</param>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TicketTypeDto>>> GetTicketTypes(int eventId, [FromQuery] bool availableOnly = false)
        {
            try
            {
                var ticketTypes = await _ticketTypeService.GetByEventIdAsync(eventId, availableOnly);
                return Ok(ticketTypes);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Get a specific ticket type by ID
        /// </summary>
        [HttpGet("{ticketTypeId}")]
        public async Task<ActionResult<TicketTypeDto>> GetTicketType(int eventId, int ticketTypeId)
        {
            var ticketType = await _ticketTypeService.GetByIdAsync(eventId, ticketTypeId);

            if (ticketType == null)
            {
                return NotFound($"Ticket type with ID {ticketTypeId} not found for event {eventId}.");
            }

            return Ok(ticketType);
        }

        /// <summary>
        /// Create a new ticket type for an event
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<TicketTypeDto>> CreateTicketType(int eventId, CreateTicketTypeDto createTicketTypeDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var ticketType = await _ticketTypeService.CreateAsync(eventId, createTicketTypeDto);
                return CreatedAtAction(nameof(GetTicketType), 
                    new { eventId = eventId, ticketTypeId = ticketType.Id }, ticketType);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Update an existing ticket type
        /// </summary>
        [HttpPut("{ticketTypeId}")]
        public async Task<IActionResult> UpdateTicketType(int eventId, int ticketTypeId, UpdateTicketTypeDto updateTicketTypeDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updated = await _ticketTypeService.UpdateAsync(eventId, ticketTypeId, updateTicketTypeDto);

                if (!updated)
                {
                    return NotFound($"Ticket type with ID {ticketTypeId} not found for event {eventId}.");
                }

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Delete a ticket type
        /// </summary>
        [HttpDelete("{ticketTypeId}")]
        public async Task<IActionResult> DeleteTicketType(int eventId, int ticketTypeId)
        {
            try
            {
                var deleted = await _ticketTypeService.DeleteAsync(eventId, ticketTypeId);

                if (!deleted)
                {
                    return NotFound($"Ticket type with ID {ticketTypeId} not found for event {eventId}.");
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
} 