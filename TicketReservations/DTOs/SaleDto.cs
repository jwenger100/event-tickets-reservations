using System.ComponentModel.DataAnnotations;
using TicketReservations.Models;

namespace TicketReservations.DTOs
{
    public class SaleDto
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public string EventTitle { get; set; } = string.Empty;
        public int TicketTypeId { get; set; }
        public string TicketTypeName { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal? ServiceFee { get; set; }
        public decimal? Tax { get; set; }
        public decimal FinalAmount { get; set; }
        public SaleStatus Status { get; set; }
        public string PaymentTransactionId { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime SaleDate { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string ConfirmationCode { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public class CreateSaleDto
    {
        public int TicketTypeId { get; set; }
        
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
        
        [StringLength(50)]
        public string PaymentMethod { get; set; } = "Credit Card";
        
        public string? Notes { get; set; }
    }

    public class CreateSaleFromReservationDto
    {
        public int ReservationId { get; set; }
        
        [StringLength(50)]
        public string PaymentMethod { get; set; } = "Credit Card";
        
        public string? Notes { get; set; }
    }

    public class ProcessPaymentDto
    {
        public int SaleId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string PaymentTransactionId { get; set; } = string.Empty;
        
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        
        public bool IsSuccessful { get; set; }
        
        public string? PaymentNotes { get; set; }
    }

    public class RefundDto
    {
        public int SaleId { get; set; }
        
        [Range(1, int.MaxValue, ErrorMessage = "Refund quantity must be at least 1")]
        public int RefundQuantity { get; set; }
        
        [Required]
        public string RefundReason { get; set; } = string.Empty;
        
        public string? RefundNotes { get; set; }
    }

    public class SaleSummaryDto
    {
        public int Id { get; set; }
        public string EventTitle { get; set; } = string.Empty;
        public string TicketTypeName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal FinalAmount { get; set; }
        public SaleStatus Status { get; set; }
        public DateTime SaleDate { get; set; }
        public string ConfirmationCode { get; set; } = string.Empty;
    }

    public class PaymentRequestDto
    {
        public int SaleId { get; set; }
        public decimal Amount { get; set; }
        public string CustomerEmail { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        // Additional payment gateway specific fields would go here
        public Dictionary<string, string> PaymentMetadata { get; set; } = new();
    }

    public class PaymentResponseDto
    {
        public bool IsSuccess { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public DateTime ProcessedAt { get; set; }
        public Dictionary<string, string> ResponseMetadata { get; set; } = new();
    }
} 