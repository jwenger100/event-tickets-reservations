using System.ComponentModel.DataAnnotations;

namespace TicketReservations.Models
{
    public class TicketType
    {
        public int Id { get; set; }
        
        public int EventId { get; set; }
        
        public Event? Event { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(300)]
        public string? Description { get; set; }
        
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }
        
        [Range(1, int.MaxValue, ErrorMessage = "Total quantity must be at least 1")]
        public int TotalQuantity { get; set; }
        
        public int AvailableQuantity { get; set; }
        
        public int ReservedQuantity { get; set; }
        
        public int SoldQuantity { get; set; }
        
        public DateTime? SaleStartDate { get; set; }
        
        public DateTime? SaleEndDate { get; set; }
        
        public int MaxPerCustomer { get; set; } = 10;
        
        public bool IsActive { get; set; } = true;
        
        public TicketCategory Category { get; set; } = TicketCategory.General;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
        
        public ICollection<Sale> Sales { get; set; } = new List<Sale>();
    }
    
    public enum TicketCategory
    {
        General = 0,
        VIP = 1,
        Premium = 2,
        EarlyBird = 3,
        Student = 4,
        Senior = 5,
        Group = 6
    }
} 