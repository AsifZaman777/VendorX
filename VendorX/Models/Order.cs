using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VendorX.Models.Enums;

namespace VendorX.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        public string OrderNumber { get; set; } = string.Empty;

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public DateTime? DeliveryDate { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [StringLength(500)]
        public string? CancellationReason { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalAmount { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        // Foreign Keys
        public int CustomerId { get; set; }
        public virtual Customer Customer { get; set; } = null!;

        public int ShopId { get; set; }
        public virtual Shop Shop { get; set; } = null!;

        // Navigation properties
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
