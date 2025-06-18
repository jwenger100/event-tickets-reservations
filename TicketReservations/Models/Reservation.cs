using System.ComponentModel.DataAnnotations;

namespace TicketReservations.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        
        public int EventId { get; set; }
        
        public Event? Event { get; set; }
        
        public int TicketTypeId { get; set; }
        
        public TicketType? TicketType { get; set; }
        
        [Required]
        [StringLength(100)]
        public string CustomerName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string CustomerEmail { get; set; } = string.Empty;
        
        [Required]
        [Phone]
        [StringLength(20)]
        public string CustomerPhone { get; set; } = string.Empty;
        
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }
        
        public decimal UnitPrice { get; set; }
        
        public decimal TotalAmount { get; set; }
        
        public ReservationStatus Status { get; set; } = ReservationStatus.Active;
        
        public DateTime ReservationDate { get; set; } = DateTime.UtcNow;
        
        // Time-based expiration
        public DateTime ExpirationDate { get; set; }
        
        public int ReservationTimeoutMinutes { get; set; } = 15; // Default 15 minutes
        
        public DateTime? ConvertedToSaleDate { get; set; }
        
        public int? SaleId { get; set; }
        
        public Sale? Sale { get; set; }
        
        [StringLength(50)]
        public string ReservationCode { get; set; } = string.Empty;
        
        public string? Notes { get; set; }
        
        // Track when reservation was released
        public DateTime? ReleasedDate { get; set; }
        
        public string? ReleasedReason { get; set; }
    }
    
    public enum ReservationStatus
    {
        Active = 0,
        Expired = 1,
        Cancelled = 2,
        ConvertedToSale = 3,
        Released = 4
    }
} 