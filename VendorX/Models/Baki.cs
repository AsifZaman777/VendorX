using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VendorX.Models.Enums;

namespace VendorX.Models
{
    public class Baki
    {
        [Key]
        public int BakiId { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }

        public BakiStatus Status { get; set; } = BakiStatus.Due;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? SettledAt { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        public TransactionType TransactionType { get; set; }

        // Foreign Keys
        public int CustomerId { get; set; }
        public virtual Customer Customer { get; set; } = null!;

        public int ShopId { get; set; }
        public virtual Shop Shop { get; set; } = null!;

        public int? POSTransactionId { get; set; }
        public virtual POSTransaction? POSTransaction { get; set; }
    }
}
