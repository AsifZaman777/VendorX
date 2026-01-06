using System.ComponentModel.DataAnnotations;

namespace VendorX.Models
{
    public class AdminNotice
    {
        [Key]
        public int NoticeId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;

        [StringLength(50)]
        public string NoticeType { get; set; } = "Info"; // Info, Warning, Danger, Success

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ExpiresAt { get; set; }

        [StringLength(50)]
        public string? TargetRole { get; set; } // ShopKeeper, Customer, or null for all
    }
}
