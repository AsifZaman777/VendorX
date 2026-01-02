using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VendorX.Models;
using VendorX.Models.Enums;
using VendorX.Services;

namespace VendorX.Areas.ShopKeeper.Controllers
{
    [Area("ShopKeeper")]
    [Authorize(Roles = "ShopKeeper")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IShopService _shopService;

        public HomeController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager, 
            IShopService shopService)
        {
            _context = context;
            _userManager = userManager;
            _shopService = shopService;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var shop = await _shopService.GetShopByUserIdAsync(user!.Id);
            
            if (shop == null)
            {
                TempData["Warning"] = "Please complete your shop setup.";
                return RedirectToAction("Create", "Shop");
            }

            // Get today's date
            var today = DateTime.Today;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);

            // Today's Sales Statistics
            var todaySales = await _context.POSTransactions
                .Where(t => t.ShopId == shop.ShopId && t.TransactionDate.Date == today)
                .SumAsync(t => (decimal?)t.TotalAmount) ?? 0;

            var todayTransactions = await _context.POSTransactions
                .Where(t => t.ShopId == shop.ShopId && t.TransactionDate.Date == today)
                .CountAsync();

            // Pending Orders
            var pendingOrders = await _context.Orders
                .Where(o => o.ShopId == shop.ShopId && o.Status == OrderStatus.Pending)
                .CountAsync();

            // Total Baki (Outstanding Credit)
            var totalBaki = await _context.BakiRecords
                .Where(b => b.ShopId == shop.ShopId && b.Status == BakiStatus.Due)
                .SumAsync(b => (decimal?)b.Amount) ?? 0;

            // Total Customers
            var totalCustomers = await _context.ShopCustomers
                .Where(sc => sc.ShopId == shop.ShopId)
                .CountAsync();

            // Monthly Sales Data for Chart (Last 7 days)
            var last7Days = Enumerable.Range(0, 7)
                .Select(i => today.AddDays(-i))
                .Reverse()
                .ToList();

            var salesByDay = await _context.POSTransactions
                .Where(t => t.ShopId == shop.ShopId && t.TransactionDate.Date >= today.AddDays(-6))
                .GroupBy(t => t.TransactionDate.Date)
                .Select(g => new { Date = g.Key, Total = g.Sum(t => t.TotalAmount) })
                .ToListAsync();

            var chartLabels = last7Days.Select(d => d.ToString("MMM dd")).ToList();
            var chartData = last7Days.Select(d => 
                salesByDay.FirstOrDefault(s => s.Date == d)?.Total ?? 0
            ).ToList();

            // Top Products (Last 30 days)
            var topProducts = await _context.POSTransactionItems
                .Include(i => i.Product)
                .Include(i => i.POSTransaction)
                .Where(i => i.POSTransaction.ShopId == shop.ShopId 
                         && i.POSTransaction.TransactionDate >= today.AddDays(-30))
                .GroupBy(i => new { i.ProductId, i.Product.ProductName })
                .Select(g => new { 
                    ProductName = g.Key.ProductName, 
                    TotalSold = g.Sum(i => i.Quantity),
                    Revenue = g.Sum(i => i.TotalPrice)
                })
                .OrderByDescending(p => p.Revenue)
                .Take(5)
                .ToListAsync();

            // Recent Transactions
            var recentTransactions = await _context.POSTransactions
                .Where(t => t.ShopId == shop.ShopId)
                .Include(t => t.Customer)
                .OrderByDescending(t => t.TransactionDate)
                .Take(5)
                .Select(t => new {
                    t.TransactionNumber,
                    CustomerName = t.Customer.FullName,
                    t.TotalAmount,
                    t.TransactionDate
                })
                .ToListAsync();

            // Low Stock Products
            var lowStockProducts = await _context.Products
                .Where(p => p.ShopId == shop.ShopId && p.StockQuantity <= 10 && p.IsActive)
                .OrderBy(p => p.StockQuantity)
                .Take(5)
                .Select(p => new { p.ProductName, p.StockQuantity })
                .ToListAsync();

            ViewBag.ShopName = shop.ShopName;
            ViewBag.TodaySales = todaySales.ToString("C");
            ViewBag.TodayTransactions = todayTransactions;
            ViewBag.PendingOrders = pendingOrders;
            ViewBag.TotalBaki = totalBaki.ToString("C");
            ViewBag.TotalCustomers = totalCustomers;
            
            // Chart Data
            ViewBag.ChartLabels = System.Text.Json.JsonSerializer.Serialize(chartLabels);
            ViewBag.ChartData = System.Text.Json.JsonSerializer.Serialize(chartData);
            
            // Lists
            ViewBag.TopProducts = topProducts;
            ViewBag.RecentTransactions = recentTransactions;
            ViewBag.LowStockProducts = lowStockProducts;

            return View();
        }
    }
}
