using System.ComponentModel.DataAnnotations;
using TicketReservations.Models;

namespace TicketReservations.DTOs
{
    public class TicketTypeDto
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public string EventTitle { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int TotalQuantity { get; set; }
        public int AvailableQuantity { get; set; }
        public int ReservedQuantity { get; set; }
        public int SoldQuantity { get; set; }
        public DateTime? SaleStartDate { get; set; }
        public DateTime? SaleEndDate { get; set; }
        public int MaxPerCustomer { get; set; }
        public bool IsActive { get; set; }
        public TicketCategory Category { get; set; }
        
        // Computed availability information
        public bool IsAvailable { get; set; }
        public string? UnavailableReason { get; set; }
    }

    public class CreateTicketTypeDto
    {
        public int EventId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(300)]
        public string? Description { get; set; }
        
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }
        
        [Range(1, int.MaxValue, ErrorMessage = "Total quantity must be at least 1")]
        public int TotalQuantity { get; set; }
        
        public DateTime? SaleStartDate { get; set; }
        
        public DateTime? SaleEndDate { get; set; }
        
        [Range(1, 50, ErrorMessage = "Max per customer must be between 1 and 50")]
        public int MaxPerCustomer { get; set; } = 10;
        
        public TicketCategory Category { get; set; } = TicketCategory.General;
    }

    public class UpdateTicketTypeDto
    {
        [StringLength(100)]
        public string? Name { get; set; }
        
        [StringLength(300)]
        public string? Description { get; set; }
        
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal? Price { get; set; }
        
        [Range(1, int.MaxValue, ErrorMessage = "Total quantity must be at least 1")]
        public int? TotalQuantity { get; set; }
        
        public DateTime? SaleStartDate { get; set; }
        
        public DateTime? SaleEndDate { get; set; }
        
        [Range(1, 50, ErrorMessage = "Max per customer must be between 1 and 50")]
        public int? MaxPerCustomer { get; set; }
        
        public bool? IsActive { get; set; }
        
        public TicketCategory? Category { get; set; }
    }


} 