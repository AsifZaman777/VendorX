using System.ComponentModel.DataAnnotations;
using VendorX.Models.Enums;

namespace VendorX.ViewModels
{
    public class POSViewModel
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;

        public List<POSItemViewModel> Items { get; set; } = new();

        [Display(Name = "Transaction Date")]
        [DataType(DataType.Date)]
        public DateTime TransactionDate { get; set; } = DateTime.Today;

        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "Amount Paid")]
        public decimal AmountPaid { get; set; }

        [Display(Name = "Amount Due")]
        public decimal AmountDue { get; set; }

        [Display(Name = "Is Credit (Baki)")]
        public bool IsCredit { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }
    }

    public class POSItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class OrderViewModel
    {
        public int OrderId { get; set; }

        [Display(Name = "Order Number")]
        public string OrderNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Customer")]
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }

        [Display(Name = "Order Date")]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Display(Name = "Delivery Date")]
        public DateTime? DeliveryDate { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [StringLength(500)]
        [Display(Name = "Cancellation Reason")]
        public string? CancellationReason { get; set; }

        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        public List<OrderItemViewModel> Items { get; set; } = new();
    }

    public class OrderItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
