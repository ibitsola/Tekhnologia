using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class Purchase
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int DigitalResourceId { get; set; }

        [ForeignKey("DigitalResourceId")]
        public DigitalResource DigitalResource { get; set; } = null!;

        [Required]
        public string UserId { get; set; } = string.Empty; // Link to the user who purchased

        [Required]
        public string StripeSessionId { get; set; } = string.Empty;  // Store Stripe's session ID

        public bool IsPaid { get; set; } = false; // Payment status

        public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;
    }
}
