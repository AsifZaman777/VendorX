using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VendorX.Models;

namespace VendorX.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize(Roles = "Customer")]
    public class ShopsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ShopsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == user!.Id);

            if (customer == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var shops = await _context.ShopCustomers
                .Where(sc => sc.CustomerId == customer.CustomerId)
                .Include(sc => sc.Shop)
                .Select(sc => sc.Shop)
                .ToListAsync();

            return View(shops);
        }

        public async Task<IActionResult> Details(int id)
        {
            var shop = await _context.Shops.FindAsync(id);
            if (shop == null)
            {
                return NotFound();
            }
            return View(shop);
        }

        public async Task<IActionResult> Purchases(int shopId)
        {
            var user = await _userManager.GetUserAsync(User);
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == user!.Id);

            if (customer == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var purchases = await _context.POSTransactions
                .Where(t => t.CustomerId == customer.CustomerId && t.ShopId == shopId)
                .Include(t => t.POSTransactionItems)
                .ThenInclude(i => i.Product)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();

            var shop = await _context.Shops.FindAsync(shopId);
            ViewBag.ShopName = shop?.ShopName;

            return View(purchases);
        }

        public async Task<IActionResult> Orders(int shopId)
        {
            var user = await _userManager.GetUserAsync(User);
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == user!.Id);

            if (customer == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var orders = await _context.Orders
                .Where(o => o.CustomerId == customer.CustomerId && o.ShopId == shopId)
                .Include(o => o.OrderItems)
                .ThenInclude(i => i.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            var shop = await _context.Shops.FindAsync(shopId);
            ViewBag.ShopName = shop?.ShopName;

            return View(orders);
        }

        public async Task<IActionResult> RegisterToShop(string qrCode)
        {
            // Parse QR code to get shop code
            if (string.IsNullOrEmpty(qrCode) || !qrCode.StartsWith("SHOP:"))
            {
                TempData["Error"] = "Invalid QR code.";
                return RedirectToAction(nameof(Index));
            }

            var shopCode = qrCode.Replace("SHOP:", "");
            var shop = await _context.Shops.FirstOrDefaultAsync(s => s.ShopCode == shopCode);

            if (shop == null)
            {
                TempData["Error"] = "Shop not found.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.GetUserAsync(User);
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == user!.Id);

            if (customer == null)
            {
                TempData["Error"] = "Customer profile not found.";
                return RedirectToAction("Profile", "Home");
            }

            // Check if already registered
            var exists = await _context.ShopCustomers
                .AnyAsync(sc => sc.ShopId == shop.ShopId && sc.CustomerId == customer.CustomerId);

            if (exists)
            {
                TempData["Warning"] = "You are already registered to this shop.";
                return RedirectToAction(nameof(Index));
            }

            // Register customer to shop
            _context.ShopCustomers.Add(new ShopCustomer
            {
                ShopId = shop.ShopId,
                CustomerId = customer.CustomerId,
                RegisteredAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Successfully registered to {shop.ShopName}!";
            return RedirectToAction(nameof(Index));
        }
    }
}
