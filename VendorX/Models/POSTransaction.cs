using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VendorX.Models
{
    public class POSTransaction
    {
        [Key]
        public int POSTransactionId { get; set; }

        public string TransactionNumber { get; set; } = string.Empty;

        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal AmountPaid { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal AmountDue { get; set; }

        public bool IsCredit { get; set; } // If true, this is a Baki transaction

        [StringLength(1000)]
        public string? Notes { get; set; }

        // Foreign Keys
        public int CustomerId { get; set; }
        public virtual Customer Customer { get; set; } = null!;

        public int ShopId { get; set; }
        public virtual Shop Shop { get; set; } = null!;

        // Navigation properties
        public virtual ICollection<POSTransactionItem> POSTransactionItems { get; set; } = new List<POSTransactionItem>();
    }
}
