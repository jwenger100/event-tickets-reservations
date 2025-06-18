using System.ComponentModel.DataAnnotations;
using TicketReservations.Models;

namespace TicketReservations.DTOs
{
    public class ReservationDto
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
        public ReservationStatus Status { get; set; }
        public DateTime ReservationDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public int ReservationTimeoutMinutes { get; set; }
        public string ReservationCode { get; set; } = string.Empty;
        public bool IsExpired => DateTime.UtcNow > ExpirationDate;
        public int? SaleId { get; set; }
    }

    public class CreateReservationDto
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
    }

    public class UpdateReservationStatusDto
    {
        public ReservationStatus Status { get; set; }
        public string? Notes { get; set; }
    }

    public class ReservationSummaryDto
    {
        public int Id { get; set; }
        public string EventTitle { get; set; } = string.Empty;
        public string TicketTypeName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal TotalAmount { get; set; }
        public ReservationStatus Status { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string ReservationCode { get; set; } = string.Empty;
        public bool IsExpired { get; set; }
    }
} 