using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VendorX.Models
{
    public class Expense
    {
        [Key]
        public int ExpenseId { get; set; }

        [Required]
        [StringLength(200)]
        public string ExpenseName { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }

        public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(200)]
        public string? Category { get; set; }

        // Foreign Key
        public int ShopId { get; set; }
        public virtual Shop Shop { get; set; } = null!;
    }
}
