using System.ComponentModel.DataAnnotations;

namespace VendorX.Models
{
    // Junction table for many-to-many relationship between Shop and Customer
    public class ShopCustomer
    {
        public int ShopId { get; set; }
        public virtual Shop Shop { get; set; } = null!;

        public int CustomerId { get; set; }
        public virtual Customer Customer { get; set; } = null!;

        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    }
}
