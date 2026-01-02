using System.ComponentModel.DataAnnotations;
using VendorX.Models.Enums;

namespace VendorX.ViewModels
{
    public class BakiViewModel
    {
        public int BakiId { get; set; }

        [Required]
        [Display(Name = "Customer")]
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }

        [Required]
        [Range(0.01, 999999.99)]
        public decimal Amount { get; set; }

        public BakiStatus Status { get; set; } = BakiStatus.Due;

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Settled At")]
        public DateTime? SettledAt { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [Display(Name = "Transaction Type")]
        public TransactionType TransactionType { get; set; }
    }

    public class BakiInvoiceViewModel
    {
        public int BakiInvoiceId { get; set; }

        [Display(Name = "Invoice Number")]
        public string InvoiceNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Customer")]
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }

        [Required]
        [Display(Name = "Month")]
        public int Month { get; set; }

        [Required]
        [Display(Name = "Year")]
        public int Year { get; set; }

        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "Is Sent")]
        public bool IsSent { get; set; }

        [Display(Name = "Sent At")]
        public DateTime? SentAt { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        public List<BakiInvoiceItemViewModel> Items { get; set; } = new();
    }

    public class BakiInvoiceItemViewModel
    {
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public DateTime TransactionDate { get; set; }
    }

    public class CustomerViewModel
    {
        public int CustomerId { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; }

        [Display(Name = "Total Baki Amount")]
        public decimal TotalBakiAmount { get; set; }
    }
}
