using System.ComponentModel.DataAnnotations;

namespace TicketReservations.DTOs
{
    public class VenueDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
        public int TotalCapacity { get; set; }
        public string? Description { get; set; }
    }

    public class CreateVenueDto
    {
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
    }

    public class UpdateVenueDto
    {
        [StringLength(100)]
        public string? Name { get; set; }
        
        [StringLength(200)]
        public string? Address { get; set; }
        
        [StringLength(50)]
        public string? City { get; set; }
        
        [StringLength(50)]
        public string? State { get; set; }
        
        [StringLength(20)]
        public string? ZipCode { get; set; }
        
        [Range(1, int.MaxValue, ErrorMessage = "Capacity must be at least 1")]
        public int? TotalCapacity { get; set; }
        
        public string? Description { get; set; }
    }
} 