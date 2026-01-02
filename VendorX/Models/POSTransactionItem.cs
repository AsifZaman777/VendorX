using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VendorX.Models
{
    public class POSTransactionItem
    {
        [Key]
        public int POSTransactionItemId { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalPrice { get; set; }

        // Foreign Keys
        public int POSTransactionId { get; set; }
        public virtual POSTransaction POSTransaction { get; set; } = null!;

        public int ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;
    }
}
