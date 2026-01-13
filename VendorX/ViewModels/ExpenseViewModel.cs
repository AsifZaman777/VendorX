using System;
using System.ComponentModel.DataAnnotations;

namespace VendorX.ViewModels
{
    public class ExpenseViewModel
    {
        public int ExpenseId { get; set; }

        [Required]
        public int ShopId { get; set; }

        [Required(ErrorMessage = "Please select a category")]
        public int ExpenseCategoryId { get; set; }
        public string? ExpenseCategoryName { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, 999999999.99)]
        public decimal Amount { get; set; }

        [Required]
        public DateTime ExpenseDate { get; set; } = DateTime.Today;

        public DateTime? DueDate { get; set; }

        [Required]
        public string Status { get; set; } = "Pending";

        [StringLength(50)]
        public string? PaymentMethod { get; set; }

        [StringLength(100)]
        public string? ReceiptNumber { get; set; }

        [StringLength(200)]
        public string? Vendor { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime? PaidAt { get; set; }
    }

    public class ExpenseCategoryViewModel
    {
        public int ExpenseCategoryId { get; set; }

        [Required]
        [StringLength(200)]
        public string CategoryName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [StringLength(100)]
        public string Icon { get; set; } = "bi-cash-stack";

        [Required]
        [StringLength(50)]
        public string Color { get; set; } = "primary";

        public decimal TotalAmount { get; set; }
        public int ExpenseCount { get; set; }
        public int PendingCount { get; set; }
        public decimal PendingAmount { get; set; }
    }

    public class FixedExpenseViewModel
    {
        public int FixedExpenseId { get; set; }

        [Required]
        public int ShopId { get; set; }

        [Required]
        public int ExpenseCategoryId { get; set; }
        public string? ExpenseCategoryName { get; set; }

        [Required]
        [StringLength(200)]
        public string ExpenseName { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 999999999.99)]
        public decimal Amount { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        public string RecurrenceType { get; set; } = "Monthly";

        [Required]
        [Range(1, 365)]
        public int RecurrenceInterval { get; set; } = 1;

        public int? DayOfMonth { get; set; }
        
        public int? DayOfWeek { get; set; }

        [Required]
        public DateTime StartDate { get; set; } = DateTime.Today;

        public DateTime? EndDate { get; set; }

        [StringLength(50)]
        public string? PaymentMethod { get; set; }

        [StringLength(200)]
        public string? Vendor { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
        
        public DateTime? NextDueDate { get; set; }
    }
}
