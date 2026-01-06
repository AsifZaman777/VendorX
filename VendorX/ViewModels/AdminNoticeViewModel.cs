using System.ComponentModel.DataAnnotations;

namespace VendorX.ViewModels
{
    public class AdminNoticeViewModel
    {
        public int NoticeId { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Title")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        [Display(Name = "Message")]
        public string Message { get; set; } = string.Empty;

        [Display(Name = "Notice Type")]
        public string NoticeType { get; set; } = "Info";

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }

        [Display(Name = "Expires At")]
        public DateTime? ExpiresAt { get; set; }

        [Display(Name = "Target Role")]
        public string? TargetRole { get; set; }
    }
}
