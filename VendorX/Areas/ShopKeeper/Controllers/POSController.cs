using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VendorX.Data;
using VendorX.Models;
using VendorX.Models.Enums;
using VendorX.Services;
using VendorX.ViewModels;

namespace VendorX.Areas.ShopKeeper.Controllers
{
    [Area("ShopKeeper")]
    [Authorize(Roles = "ShopKeeper")]
    public class POSController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IShopService _shopService;
        private readonly IEmailService _emailService;
        private readonly IWhatsAppService _whatsAppService;

        public POSController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IShopService shopService,
            IEmailService emailService,
            IWhatsAppService whatsAppService)
        {
            _context = context;
            _userManager = userManager;
            _shopService = shopService;
            _emailService = emailService;
            _whatsAppService = whatsAppService;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var shop = await _shopService.GetShopByUserIdAsync(user!.Id);

            if (shop == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var customers = await _context.ShopCustomers
                .Where(sc => sc.ShopId == shop.ShopId)
                .Include(sc => sc.Customer)
                .Select(sc => sc.Customer)
                .ToListAsync();

            ViewBag.Customers = customers;
            ViewBag.ShopId = shop.ShopId;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts(int shopId)
        {
            var products = await _context.Products
                .Where(p => p.ShopId == shopId && p.IsActive)
                .Select(p => new
                {
                    p.ProductId,
                    p.ProductName,
                    p.Price,
                    p.StockQuantity,
                    CategoryName = p.Category.CategoryName
                })
                .ToListAsync();

            return Json(products);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTransaction(POSViewModel model)
        {
            // Before creating transaction
            if (model.AmountDue > 0)
            {
                model.IsCredit = true;
            }

            var user = await _userManager.GetUserAsync(User);
            var shop = await _shopService.GetShopByUserIdAsync(user!.Id);

            if (shop == null)
            {
                return BadRequest("Shop not found");
            }

            // Use the selected date with current time
            // If TransactionDate is not set or is default, use current datetime
            DateTime transactionDateTime;
            if (model.TransactionDate == default || model.TransactionDate.Year < 2020)
            {
                transactionDateTime = DateTime.Now;
            }
            else
            {
                // Combine the selected date with current time
                transactionDateTime = model.TransactionDate.Date.Add(DateTime.Now.TimeOfDay);
            }

            var transaction = new POSTransaction
            {
                TransactionNumber = $"POS-{DateTime.Now:yyyyMMddHHmmss}",
                CustomerId = model.CustomerId,
                ShopId = shop.ShopId,
                TotalAmount = model.TotalAmount,
                AmountPaid = model.AmountPaid,
                AmountDue = model.AmountDue,
                IsCredit = model.IsCredit,
                Notes = model.Notes,
                TransactionDate = transactionDateTime
            };

            _context.POSTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            foreach (var item in model.Items)
            {
                var transactionItem = new POSTransactionItem
                {
                    POSTransactionId = transaction.POSTransactionId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.TotalPrice
                };
                _context.POSTransactionItems.Add(transactionItem);

                // Update product stock
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    product.StockQuantity -= item.Quantity;
                }
            }

            // If it's a credit transaction, create Baki record
            if (model.IsCredit && model.AmountDue > 0)
            {
                // Create a new Baki record for each credit transaction
                // This allows tracking by date and linking to specific POS transactions
                var baki = new Baki
                {
                    CustomerId = model.CustomerId,
                    ShopId = shop.ShopId,
                    Amount = model.AmountDue,
                    Status = BakiStatus.Due,
                    Description = $"POS Transaction - {transaction.TransactionNumber}: ${model.AmountDue:F2}",
                    TransactionType = TransactionType.Purchase,
                    POSTransactionId = transaction.POSTransactionId,
                    CreatedAt = transactionDateTime
                };
                _context.BakiRecords.Add(baki);
            }

            await _context.SaveChangesAsync();

            // Send notification to customer
            var customer = await _context.Customers.FindAsync(model.CustomerId);
            if (customer != null)
            {
                var message = $"Purchase completed. Transaction #: {transaction.TransactionNumber}. Total: {model.TotalAmount:C}";
                
                if (model.IsCredit && model.AmountDue > 0)
                {
                    message += $"\nAmount Due: {model.AmountDue:C}";
                }
                
                if (!string.IsNullOrEmpty(customer.Email))
                {
                    await _emailService.SendEmailAsync(customer.Email, "Purchase Receipt", message);
                }

                if (!string.IsNullOrEmpty(customer.PhoneNumber))
                {
                    await _whatsAppService.SendWhatsAppMessageAsync(customer.PhoneNumber, message);
                }
            }

            TempData["Success"] = "Transaction completed successfully!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Transactions()
        {
            var user = await _userManager.GetUserAsync(User);
            var shop = await _shopService.GetShopByUserIdAsync(user!.Id);

            if (shop == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var transactions = await _context.POSTransactions
                .Where(t => t.ShopId == shop.ShopId)
                .Include(t => t.Customer)
                .Include(t => t.POSTransactionItems)
                .ThenInclude(i => i.Product)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();

            return View(transactions);
        }
    }
}
