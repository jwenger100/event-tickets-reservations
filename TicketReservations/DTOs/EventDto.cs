using System.ComponentModel.DataAnnotations;
using TicketReservations.Models;

namespace TicketReservations.DTOs
{
    public class EventDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public DateTime? DoorsOpen { get; set; }
        public int VenueId { get; set; }
        public VenueDto? Venue { get; set; }
        public EventStatus Status { get; set; }
        public string? Artist { get; set; }
        public string? Genre { get; set; }
        public int MinimumAge { get; set; }
        public string? ImageUrl { get; set; }
        public List<TicketTypeDto> TicketTypes { get; set; } = new();
    }

    public class CreateEventDto
    {
        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        
        public DateTime EventDate { get; set; }
        
        public DateTime? DoorsOpen { get; set; }
        
        public int VenueId { get; set; }
        
        [StringLength(100)]
        public string? Artist { get; set; }
        
        [StringLength(50)]
        public string? Genre { get; set; }
        
        [Range(0, 99)]
        public int MinimumAge { get; set; } = 0;
        
        public string? ImageUrl { get; set; }
    }

    public class UpdateEventDto
    {
        [StringLength(150)]
        public string? Title { get; set; }
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public DateTime? EventDate { get; set; }
        
        public DateTime? DoorsOpen { get; set; }
        
        public int? VenueId { get; set; }
        
        public EventStatus? Status { get; set; }
        
        [StringLength(100)]
        public string? Artist { get; set; }
        
        [StringLength(50)]
        public string? Genre { get; set; }
        
        [Range(0, 99)]
        public int? MinimumAge { get; set; }
        
        public string? ImageUrl { get; set; }
    }

    public class EventSummaryDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string VenueName { get; set; } = string.Empty;
        public string? Artist { get; set; }
        public EventStatus Status { get; set; }
        public int TotalTicketTypes { get; set; }
        public decimal? LowestPrice { get; set; }
        public int TotalAvailableTickets { get; set; }
    }
} 