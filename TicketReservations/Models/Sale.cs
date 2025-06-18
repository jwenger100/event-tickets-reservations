using System.ComponentModel.DataAnnotations;

namespace TicketReservations.Models
{
    public class Sale
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
        
        public decimal? ServiceFee { get; set; }
        
        public decimal? Tax { get; set; }
        
        public decimal FinalAmount { get; set; }
        
        public SaleStatus Status { get; set; } = SaleStatus.Pending;
        
        [StringLength(100)]
        public string PaymentTransactionId { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string PaymentMethod { get; set; } = string.Empty;
        
        public DateTime SaleDate { get; set; } = DateTime.UtcNow;
        
        public DateTime? PaymentDate { get; set; }
        
        [StringLength(50)]
        public string ConfirmationCode { get; set; } = string.Empty;
        
        public string? Notes { get; set; }
        
        // For refunds
        public int? RefundedQuantity { get; set; }
        
        public decimal? RefundAmount { get; set; }
        
        public DateTime? RefundDate { get; set; }
        
        public string? RefundReason { get; set; }
    }
    
    public enum SaleStatus
    {
        Pending = 0,
        PaymentProcessing = 1,
        Completed = 2,
        Failed = 3,
        Cancelled = 4,
        Refunded = 5,
        PartiallyRefunded = 6
    }
} 