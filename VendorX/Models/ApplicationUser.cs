using Microsoft.AspNetCore.Identity;
using VendorX.Models.Enums;

namespace VendorX.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual Shop? Shop { get; set; }
        public virtual Customer? Customer { get; set; }
    }
}
