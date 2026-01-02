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
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IShopService _shopService;
        private readonly IEmailService _emailService;
        private readonly IWhatsAppService _whatsAppService;

        public OrdersController(
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

        public async Task<IActionResult> Index(OrderStatus? status = null)
        {
            var user = await _userManager.GetUserAsync(User);
            var shop = await _shopService.GetShopByUserIdAsync(user!.Id);

            if (shop == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var query = _context.Orders
                .Where(o => o.ShopId == shop.ShopId)
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .ThenInclude(i => i.Product)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status.Value);
            }

            var orders = await query.OrderByDescending(o => o.OrderDate).ToListAsync();

            ViewBag.StatusFilter = status;
            return View(orders);
        }

        public async Task<IActionResult> Create()
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

            var products = await _context.Products
                .Where(p => p.ShopId == shop.ShopId && p.IsActive)
                .ToListAsync();

            ViewBag.Customers = customers;
            ViewBag.Products = products;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            var shop = await _shopService.GetShopByUserIdAsync(user!.Id);

            if (shop == null)
            {
                return BadRequest("Shop not found");
            }

            var order = new Order
            {
                OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}",
                CustomerId = model.CustomerId,
                ShopId = shop.ShopId,
                OrderDate = DateTime.UtcNow,
                DeliveryDate = model.DeliveryDate,
                Status = OrderStatus.Pending,
                TotalAmount = model.TotalAmount,
                Notes = model.Notes
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            foreach (var item in model.Items)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.OrderId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.TotalPrice
                };
                _context.OrderItems.Add(orderItem);
            }

            await _context.SaveChangesAsync();

            // Send notification to customer
            var customer = await _context.Customers.FindAsync(model.CustomerId);
            if (customer != null)
            {
                var message = $"New order created. Order #: {order.OrderNumber}. Total: {model.TotalAmount:C}";
                
                if (!string.IsNullOrEmpty(customer.Email))
                {
                    await _emailService.SendEmailAsync(customer.Email, "New Order", message);
                }

                if (!string.IsNullOrEmpty(customer.PhoneNumber))
                {
                    await _whatsAppService.SendWhatsAppMessageAsync(customer.PhoneNumber, message);
                }
            }

            TempData["Success"] = "Order created successfully!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatus status, string? reason = null)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            order.Status = status;

            if (status == OrderStatus.Delivered)
            {
                order.DeliveryDate = DateTime.UtcNow;
            }
            else if (status == OrderStatus.Cancelled)
            {
                order.CancellationReason = reason;
            }

            await _context.SaveChangesAsync();

            // Send notification to customer
            var customer = order.Customer;
            if (customer != null)
            {
                var message = $"Order {order.OrderNumber} status updated to: {status}";
                if (!string.IsNullOrEmpty(reason))
                {
                    message += $". Reason: {reason}";
                }
                
                if (!string.IsNullOrEmpty(customer.Email))
                {
                    await _emailService.SendEmailAsync(customer.Email, "Order Status Update", message);
                }

                if (!string.IsNullOrEmpty(customer.PhoneNumber))
                {
                    await _whatsAppService.SendWhatsAppMessageAsync(customer.PhoneNumber, message);
                }
            }

            TempData["Success"] = "Order status updated successfully!";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
