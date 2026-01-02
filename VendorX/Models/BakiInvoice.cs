using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VendorX.Models
{
    public class BakiInvoice
    {
        [Key]
        public int BakiInvoiceId { get; set; }

        public string InvoiceNumber { get; set; } = string.Empty;

        public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;

        public int Month { get; set; }
        public int Year { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalAmount { get; set; }

        public bool IsSent { get; set; } = false;

        public DateTime? SentAt { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        // Foreign Keys
        public int CustomerId { get; set; }
        public virtual Customer Customer { get; set; } = null!;

        public int ShopId { get; set; }
        public virtual Shop Shop { get; set; } = null!;

        // Navigation properties
        public virtual ICollection<BakiInvoiceItem> BakiInvoiceItems { get; set; } = new List<BakiInvoiceItem>();
    }
}
