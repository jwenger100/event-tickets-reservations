using Microsoft.AspNetCore.Mvc;
using TicketReservations.Core.Services;
using TicketReservations.DTOs;

namespace TicketReservations.Controllers
{
    [ApiController]
    [ApiVersion("1")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class VenuesController : ControllerBase
    {
        private readonly IVenueService _venueService;
        private readonly ILogger<VenuesController> _logger;

        public VenuesController(IVenueService venueService, ILogger<VenuesController> logger)
        {
            _venueService = venueService;
            _logger = logger;
        }

        /// <summary>
        /// Get all venues
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VenueDto>>> GetVenues()
        {
            var venues = await _venueService.GetAllAsync();
            return Ok(venues);
        }

        /// <summary>
        /// Get a specific venue by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<VenueDto>> GetVenue(int id)
        {
            var venue = await _venueService.GetByIdAsync(id);

            if (venue == null)
            {
                return NotFound($"Venue with ID {id} not found.");
            }

            return Ok(venue);
        }

        /// <summary>
        /// Create a new venue
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<VenueDto>> CreateVenue(CreateVenueDto createVenueDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var venue = await _venueService.CreateAsync(createVenueDto);
                return CreatedAtAction(nameof(GetVenue), new { id = venue.Id }, venue);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Update an existing venue
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVenue(int id, UpdateVenueDto updateVenueDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updated = await _venueService.UpdateAsync(id, updateVenueDto);

                if (!updated)
                {
                    return NotFound($"Venue with ID {id} not found.");
                }

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Delete a venue
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVenue(int id)
        {
            try
            {
                var deleted = await _venueService.DeleteAsync(id);

                if (!deleted)
                {
                    return NotFound($"Venue with ID {id} not found.");
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