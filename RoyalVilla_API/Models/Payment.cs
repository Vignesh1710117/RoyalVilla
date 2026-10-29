using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoyalVilla_API.Models
{
    public class Payment
    {
        public int Id { get; set; }

        // Relationship with the existing booking
        [Required]
        public int bookVillaDId { get; set; }

        [ForeignKey(nameof(bookVillaDId))]
        public BookVilla? BookVilla { get; set; }

        // Payment amount and currency
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(3)]
        public string Currency { get; set; } = "INR";

        // Payment gateway details
        [Required]
        [MaxLength(30)]
        public string PaymentGateway { get; set; } = "Razorpay";

        [MaxLength(100)]
        public string? GatewayOrderId { get; set; }

        [MaxLength(100)]
        public string? GatewayPaymentId { get; set; }

        // Pending, Successful, Failed, or Refunded
        [Required]
        [MaxLength(20)]
        public string PaymentStatus { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [MaxLength(500)]
        public string? FailureReason { get; set; }
    }
}
