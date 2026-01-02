using System.ComponentModel.DataAnnotations;

namespace VendorX.Models
{
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }

        [Required]
        [StringLength(500)]
        public string Message { get; set; } = string.Empty;

        public bool IsSent { get; set; } = false;

        public DateTime? SentAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(50)]
        public string NotificationType { get; set; } = string.Empty; // Email, WhatsApp, Both

        [EmailAddress]
        public string? EmailAddress { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        [StringLength(200)]
        public string? Subject { get; set; }

        public string? ErrorMessage { get; set; }

        // Foreign Key
        public int? CustomerId { get; set; }
        public virtual Customer? Customer { get; set; }
    }
}
