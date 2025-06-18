using Microsoft.AspNetCore.Mvc;
using TicketReservations.Core.Services;
using TicketReservations.DTOs;
using TicketReservations.Models;

namespace TicketReservations.Controllers
{
    [ApiController]
    [ApiVersion("1")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly IEventService _eventService;
        private readonly ILogger<EventsController> _logger;

        public EventsController(IEventService eventService, ILogger<EventsController> logger)
        {
            _eventService = eventService;
            _logger = logger;
        }

        /// <summary>
        /// Get all events with their venue information
        /// </summary>
        /// <param name="status">Filter by event status</param>
        /// <param name="onSale">Filter to only show events currently on sale (with available tickets)</param>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventSummaryDto>>> GetEvents(
            [FromQuery] EventStatus? status = null,
            [FromQuery] bool onSale = false)
        {
            IEnumerable<EventSummaryDto> events;
            
            if (onSale)
            {
                events = await _eventService.GetOnSaleAsync();
            }
            else
            {
                events = await _eventService.GetAllAsync(status);
            }
            
            return Ok(events);
        }

        /// <summary>
        /// Get a specific event by ID with full details
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<EventDto>> GetEvent(int id)
        {
            var eventDto = await _eventService.GetByIdAsync(id);

            if (eventDto == null)
            {
                return NotFound($"Event with ID {id} not found.");
            }

            return Ok(eventDto);
        }

        /// <summary>
        /// Create a new event
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<EventDto>> CreateEvent(CreateEventDto createEventDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var eventDto = await _eventService.CreateAsync(createEventDto);
                return CreatedAtAction(nameof(GetEvent), new { id = eventDto.Id }, eventDto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Update an existing event
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(int id, UpdateEventDto updateEventDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updated = await _eventService.UpdateAsync(id, updateEventDto);
                
                if (!updated)
                {
                    return NotFound($"Event with ID {id} not found.");
                }

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Delete an event
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            try
            {
                var deleted = await _eventService.DeleteAsync(id);
                
                if (!deleted)
                {
                    return NotFound($"Event with ID {id} not found.");
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