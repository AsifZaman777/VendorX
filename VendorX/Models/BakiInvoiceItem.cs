using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VendorX.Models
{
    public class BakiInvoiceItem
    {
        [Key]
        public int BakiInvoiceItemId { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public DateTime TransactionDate { get; set; }

        // Foreign Keys
        public int BakiInvoiceId { get; set; }
        public virtual BakiInvoice BakiInvoice { get; set; } = null!;

        public int BakiId { get; set; }
        public virtual Baki Baki { get; set; } = null!;
    }
}
