using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VendorX.Models;
using VendorX.Models.Enums;
using VendorX.Services;
using VendorX.ViewModels;

namespace VendorX.Areas.ShopKeeper.Controllers
{
    [Area("ShopKeeper")]
    [Authorize(Roles = "ShopKeeper")]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IShopService _shopService;

        public ReportsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IShopService shopService)
        {
            _context = context;
            _userManager = userManager;
            _shopService = shopService;
        }

        public async Task<IActionResult> Daily(DateTime? date = null)
        {
            var user = await _userManager.GetUserAsync(User);
            var shop = await _shopService.GetShopByUserIdAsync(user!.Id);

            if (shop == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var reportDate = date ?? DateTime.Today;
            var startDate = reportDate.Date;
            var endDate = startDate.AddDays(1);

            var transactions = await _context.POSTransactions
                .Where(t => t.ShopId == shop.ShopId && t.TransactionDate >= startDate && t.TransactionDate < endDate)
                .Include(t => t.Customer)
                .ToListAsync();

            var expenses = await _context.Expenses
                .Where(e => e.ShopId == shop.ShopId && e.ExpenseDate >= startDate && e.ExpenseDate < endDate)
                .ToListAsync();

            var orders = await _context.Orders
                .Where(o => o.ShopId == shop.ShopId && o.OrderDate >= startDate && o.OrderDate < endDate)
                .CountAsync();

            var totalBaki = await _context.BakiRecords
                .Where(b => b.ShopId == shop.ShopId && b.CreatedAt >= startDate && b.CreatedAt < endDate && b.Status == BakiStatus.Due)
                .SumAsync(b => b.Amount);

            var model = new DailyReportViewModel
            {
                ReportDate = reportDate,
                TotalSales = transactions.Sum(t => t.TotalAmount),
                TotalExpenses = expenses.Sum(e => e.Amount),
                TotalProfit = transactions.Sum(t => t.TotalAmount) - expenses.Sum(e => e.Amount),
                TotalTransactions = transactions.Count,
                TotalOrders = orders,
                TotalBakiAmount = totalBaki,
                Transactions = transactions.Select(t => new TransactionSummary
                {
                    TransactionNumber = t.TransactionNumber,
                    CustomerName = t.Customer.FullName,
                    TransactionDate = t.TransactionDate,
                    Amount = t.TotalAmount,
                    IsCredit = t.IsCredit
                }).ToList()
            };

            return View(model);
        }

        public async Task<IActionResult> Baki(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var user = await _userManager.GetUserAsync(User);
            var shop = await _shopService.GetShopByUserIdAsync(user!.Id);

            if (shop == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var from = fromDate ?? DateTime.Today.AddMonths(-1);
            var to = toDate ?? DateTime.Today;

            var bakiRecords = await _context.BakiRecords
                .Where(b => b.ShopId == shop.ShopId && b.CreatedAt >= from && b.CreatedAt <= to)
                .Include(b => b.Customer)
                .ToListAsync();

            var customerBakis = bakiRecords
                .GroupBy(b => new { b.CustomerId, b.Customer.FullName })
                .Select(g => new CustomerBakiSummary
                {
                    CustomerName = g.Key.FullName,
                    DueAmount = g.Where(b => b.Status == BakiStatus.Due).Sum(b => b.Amount),
                    SettledAmount = g.Where(b => b.Status == BakiStatus.Settled).Sum(b => b.Amount),
                    TotalAmount = g.Sum(b => b.Amount)
                })
                .ToList();

            var model = new BakiReportViewModel
            {
                FromDate = from,
                ToDate = to,
                TotalDue = customerBakis.Sum(c => c.DueAmount),
                TotalSettled = customerBakis.Sum(c => c.SettledAmount),
                CustomerBakis = customerBakis
            };

            return View(model);
        }

        public async Task<IActionResult> Expense(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var user = await _userManager.GetUserAsync(User);
            var shop = await _shopService.GetShopByUserIdAsync(user!.Id);

            if (shop == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var from = fromDate ?? DateTime.Today.AddMonths(-1);
            var to = toDate ?? DateTime.Today;

            var expenses = await _context.Expenses
                .Where(e => e.ShopId == shop.ShopId && e.ExpenseDate >= from && e.ExpenseDate <= to)
                .OrderByDescending(e => e.ExpenseDate)
                .ToListAsync();

            var expensesByCategory = expenses
                .GroupBy(e => e.Category ?? "Uncategorized")
                .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));

            var model = new ExpenseReportViewModel
            {
                FromDate = from,
                ToDate = to,
                TotalExpenses = expenses.Sum(e => e.Amount),
                Expenses = expenses.Select(e => new ExpenseSummary
                {
                    ExpenseName = e.ExpenseName,
                    Category = e.Category,
                    ExpenseDate = e.ExpenseDate,
                    Amount = e.Amount
                }).ToList(),
                ExpensesByCategory = expensesByCategory
            };

            return View(model);
        }

        public async Task<IActionResult> ProfitExpense(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var user = await _userManager.GetUserAsync(User);
            var shop = await _shopService.GetShopByUserIdAsync(user!.Id);

            if (shop == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var from = fromDate ?? DateTime.Today.AddMonths(-1);
            var to = toDate ?? DateTime.Today;

            var totalRevenue = await _context.POSTransactions
                .Where(t => t.ShopId == shop.ShopId && t.TransactionDate >= from && t.TransactionDate <= to)
                .SumAsync(t => t.TotalAmount);

            var totalExpenses = await _context.Expenses
                .Where(e => e.ShopId == shop.ShopId && e.ExpenseDate >= from && e.ExpenseDate <= to)
                .SumAsync(e => e.Amount);

            var netProfit = totalRevenue - totalExpenses;
            var profitMargin = totalRevenue > 0 ? (netProfit / totalRevenue) * 100 : 0;

            var model = new ProfitExpenseReportViewModel
            {
                FromDate = from,
                ToDate = to,
                TotalRevenue = totalRevenue,
                TotalExpenses = totalExpenses,
                NetProfit = netProfit,
                ProfitMargin = profitMargin
            };

            return View(model);
        }
    }
}
