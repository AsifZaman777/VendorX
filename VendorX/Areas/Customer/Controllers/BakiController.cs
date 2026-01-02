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
    public class BakiController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BakiController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> History(int? shopId = null)
        {
            var user = await _userManager.GetUserAsync(User);
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == user!.Id);

            if (customer == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var query = _context.BakiRecords
                .Where(b => b.CustomerId == customer.CustomerId)
                .Include(b => b.Shop)
                .AsQueryable();

            if (shopId.HasValue)
            {
                query = query.Where(b => b.ShopId == shopId.Value);
            }

            var bakiRecords = await query
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            var shops = await _context.ShopCustomers
                .Where(sc => sc.CustomerId == customer.CustomerId)
                .Include(sc => sc.Shop)
                .Select(sc => sc.Shop)
                .ToListAsync();

            ViewBag.Shops = shops;
            ViewBag.SelectedShopId = shopId;

            return View(bakiRecords);
        }

        public async Task<IActionResult> Summary()
        {
            var user = await _userManager.GetUserAsync(User);
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == user!.Id);

            if (customer == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var bakiByShop = await _context.BakiRecords
                .Where(b => b.CustomerId == customer.CustomerId)
                .Include(b => b.Shop)
                .GroupBy(b => new { b.ShopId, b.Shop.ShopName })
                .Select(g => new
                {
                    ShopId = g.Key.ShopId,
                    ShopName = g.Key.ShopName,
                    DueAmount = g.Where(b => b.Status == BakiStatus.Due).Sum(b => b.Amount),
                    SettledAmount = g.Where(b => b.Status == BakiStatus.Settled).Sum(b => b.Amount),
                    TotalAmount = g.Sum(b => b.Amount)
                })
                .ToListAsync();

            return View(bakiByShop);
        }

        public async Task<IActionResult> Alerts()
        {
            var user = await _userManager.GetUserAsync(User);
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == user!.Id);

            if (customer == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var invoices = await _context.BakiInvoices
                .Where(i => i.CustomerId == customer.CustomerId)
                .Include(i => i.Shop)
                .Include(i => i.BakiInvoiceItems)
                .OrderByDescending(i => i.InvoiceDate)
                .ToListAsync();

            return View(invoices);
        }

        public async Task<IActionResult> InvoiceDetails(int id)
        {
            var invoice = await _context.BakiInvoices
                .Include(i => i.Shop)
                .Include(i => i.BakiInvoiceItems)
                .ThenInclude(ii => ii.Baki)
                .FirstOrDefaultAsync(i => i.BakiInvoiceId == id);

            if (invoice == null)
            {
                return NotFound();
            }

            return View(invoice);
        }
    }
}
