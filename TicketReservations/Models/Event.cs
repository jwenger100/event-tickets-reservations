using System.ComponentModel.DataAnnotations;

namespace TicketReservations.Models
{
    public class Event
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        
        public DateTime EventDate { get; set; }
        
        public DateTime? DoorsOpen { get; set; }
        
        public int VenueId { get; set; }
        
        public Venue? Venue { get; set; }
        
        public EventStatus Status { get; set; } = EventStatus.Scheduled;
        
        [StringLength(100)]
        public string? Artist { get; set; }
        
        [StringLength(50)]
        public string? Genre { get; set; }
        
        public int MinimumAge { get; set; } = 0;
        
        public string? ImageUrl { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public ICollection<TicketType> TicketTypes { get; set; } = new List<TicketType>();
        
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
        
        public ICollection<Sale> Sales { get; set; } = new List<Sale>();
    }
    
    public enum EventStatus
    {
        Scheduled = 0,
        OnSale = 1,
        SoldOut = 2,
        Cancelled = 3,
        Completed = 4
    }
} 