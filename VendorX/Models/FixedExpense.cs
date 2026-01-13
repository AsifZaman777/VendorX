using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VendorX.Models
{
    public class FixedExpense
    {
        [Key]
        public int FixedExpenseId { get; set; }

        [Required]
        [StringLength(200)]
        public string ExpenseName { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        // Foreign Keys
        public int ShopId { get; set; }
        public virtual Shop Shop { get; set; } = null!;

        public int ExpenseCategoryId { get; set; }
        public virtual ExpenseCategory ExpenseCategory { get; set; } = null!;

        // Recurrence settings
        [Required]
        [StringLength(50)]
        public string RecurrenceType { get; set; } = "Monthly"; // Daily, Weekly, Monthly, Yearly

        public int RecurrenceInterval { get; set; } = 1; // Every X days/weeks/months/years

        public int? DayOfMonth { get; set; } // For monthly: 1-31
        
        public int? DayOfWeek { get; set; } // For weekly: 0-6 (Sunday-Saturday)

        public DateTime StartDate { get; set; } = DateTime.Today;

        public DateTime? EndDate { get; set; } // Null = no end date

        public DateTime? LastGenerated { get; set; } // Last time expense was auto-generated

        public DateTime? NextDueDate { get; set; } // Next scheduled generation

        public bool IsActive { get; set; } = true;

        // Additional fields
        [StringLength(50)]
        public string? PaymentMethod { get; set; }

        [StringLength(200)]
        public string? Vendor { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<Expense> GeneratedExpenses { get; set; } = new List<Expense>();
    }
}
