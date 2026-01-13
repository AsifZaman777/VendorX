using System.ComponentModel.DataAnnotations;

namespace VendorX.Models
{
    public class ExpenseCategory
    {
        [Key]
        public int ExpenseCategoryId { get; set; }

        [Required]
        [StringLength(200)]
        public string CategoryName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(100)]
        public string Icon { get; set; } = "bi-cash-stack"; // Bootstrap icon class

        [StringLength(50)]
        public string Color { get; set; } = "primary"; // Bootstrap color class

        public bool IsDefault { get; set; } = false; // System-defined categories

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Foreign Key - null for default categories
        public int? ShopId { get; set; }
        public virtual Shop? Shop { get; set; }

        // Navigation properties
        public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();
        public virtual ICollection<FixedExpense> FixedExpenses { get; set; } = new List<FixedExpense>();
    }
}
