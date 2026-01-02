using System.ComponentModel.DataAnnotations;

namespace VendorX.Models
{
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }

        [Required]
        [StringLength(200)]
        public string FullName { get; set; } = string.Empty;

        [Phone]
        public string? PhoneNumber { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Foreign Key
        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }

        // Navigation properties
        public virtual ICollection<ShopCustomer> ShopCustomers { get; set; } = new List<ShopCustomer>();
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<POSTransaction> POSTransactions { get; set; } = new List<POSTransaction>();
        public virtual ICollection<Baki> BakiRecords { get; set; } = new List<Baki>();
    }
}
