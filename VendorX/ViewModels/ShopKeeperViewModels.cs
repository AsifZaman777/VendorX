using System.ComponentModel.DataAnnotations;
using VendorX.Models;

namespace VendorX.ViewModels
{
    public class ShopViewModel
    {
        public int ShopId { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Shop Name")]
        public string ShopName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Address { get; set; }

        [Phone]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [Display(Name = "QR Code")]
        public string? QRCode { get; set; }

        [Display(Name = "Shop Code")]
        public string? ShopCode { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; }
    }

    public class ProductViewModel
    {
        public int ProductId { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Product Name")]
        public string ProductName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [Range(0.01, 999999.99)]
        [Display(Name = "Price")]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        [Display(Name = "Stock Quantity")]
        public int StockQuantity { get; set; }

        [Display(Name = "Image")]
        public string? ImageUrl { get; set; }

        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        public string? CategoryName { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; }
    }

    public class CategoryViewModel
    {
        public int CategoryId { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Category Name")]
        public string CategoryName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; }
    }
}
