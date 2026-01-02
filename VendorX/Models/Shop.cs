using System.ComponentModel.DataAnnotations;

namespace VendorX.Models
{
    public class Shop
    {
        [Key]
        public int ShopId { get; set; }

        [Required]
        [StringLength(200)]
        public string ShopName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Address { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        public string QRCode { get; set; } = string.Empty; // Store QR code as base64 or path

        public string? ShopCode { get; set; } // Unique code for shop

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Foreign Key
        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }

        // Navigation properties
        public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<POSTransaction> POSTransactions { get; set; } = new List<POSTransaction>();
        public virtual ICollection<Baki> BakiRecords { get; set; } = new List<Baki>();
        public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();
        public virtual ICollection<ShopCustomer> ShopCustomers { get; set; } = new List<ShopCustomer>();
    }
}
