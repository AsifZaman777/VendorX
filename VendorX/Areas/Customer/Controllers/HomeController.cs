using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VendorX.Models;
using VendorX.Models.Enums;

namespace VendorX.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize(Roles = "Customer")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
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
                TempData["Warning"] = "Please complete your profile.";
                return RedirectToAction("Profile");
            }

            // Get statistics
            var shopCount = await _context.ShopCustomers
                .Where(sc => sc.CustomerId == customer.CustomerId)
                .CountAsync();

            var purchaseCount = await _context.POSTransactions
                .Where(t => t.CustomerId == customer.CustomerId)
                .CountAsync();

            var totalBaki = await _context.BakiRecords
                .Where(b => b.CustomerId == customer.CustomerId && b.Status == BakiStatus.Due)
                .SumAsync(b => (decimal?)b.Amount) ?? 0;

            var pendingOrders = await _context.Orders
                .Where(o => o.CustomerId == customer.CustomerId && o.Status == OrderStatus.Pending)
                .CountAsync();

            ViewBag.ShopCount = shopCount;
            ViewBag.PurchaseCount = purchaseCount;
            ViewBag.TotalBaki = totalBaki;
            ViewBag.PendingOrders = pendingOrders;

            return View();
        }

        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == user!.Id);

            return View(customer);
        }
    }
}
