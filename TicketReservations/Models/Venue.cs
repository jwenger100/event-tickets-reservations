using System.ComponentModel.DataAnnotations;

namespace TicketReservations.Models
{
    public class Venue
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [StringLength(200)]
        public string Address { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string City { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string State { get; set; } = string.Empty;
        
        [StringLength(20)]
        public string ZipCode { get; set; } = string.Empty;
        
        [Range(1, int.MaxValue, ErrorMessage = "Capacity must be at least 1")]
        public int TotalCapacity { get; set; }
        
        public string? Description { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public ICollection<Event> Events { get; set; } = new List<Event>();
    }
} 