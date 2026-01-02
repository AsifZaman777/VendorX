using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VendorX.Models;
using VendorX.Models.Enums;
using VendorX.Services;
using VendorX.ViewModels;

namespace VendorX.Areas.ShopKeeper.Controllers
{
    [Area("ShopKeeper")]
    [Authorize(Roles = "ShopKeeper")]
    public class BakiController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IShopService _shopService;
        private readonly IBakiService _bakiService;
        private readonly ICustomerService _customerService;

        public BakiController(
            UserManager<ApplicationUser> userManager,
            IShopService shopService,
            IBakiService bakiService,
            ICustomerService customerService)
        {
            _userManager = userManager;
            _shopService = shopService;
            _bakiService = bakiService;
            _customerService = customerService;
        }

        public async Task<IActionResult> Index(BakiStatus? status = null)
        {
            var user = await _userManager.GetUserAsync(User);
            var shop = await _shopService.GetShopByUserIdAsync(user!.Id);

            if (shop == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var bakiRecords = await _bakiService.GetAllBakiAsync(shop.ShopId, status);
            ViewBag.StatusFilter = status;

            return View(bakiRecords);
        }

        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            var shop = await _shopService.GetShopByUserIdAsync(user!.Id);

            if (shop == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var customers = await _customerService.GetAllCustomersAsync(shop.ShopId);
            ViewBag.Customers = customers;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BakiViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                var shop = await _shopService.GetShopByUserIdAsync(user!.Id);

                if (shop == null)
                {
                    return BadRequest("Shop not found");
                }

                await _bakiService.CreateBakiAsync(model, shop.ShopId);
                TempData["Success"] = "Baki record created successfully!";
                return RedirectToAction(nameof(Index));
            }

            var user2 = await _userManager.GetUserAsync(User);
            var shop2 = await _shopService.GetShopByUserIdAsync(user2!.Id);
            var customers = await _customerService.GetAllCustomersAsync(shop2!.ShopId);
            ViewBag.Customers = customers;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Settle(int id)
        {
            var result = await _bakiService.SettleBakiAsync(id);
            if (result)
            {
                TempData["Success"] = "Baki settled successfully!";
            }
            else
            {
                TempData["Error"] = "Unable to settle baki.";
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> CustomerBaki(int customerId)
        {
            var user = await _userManager.GetUserAsync(User);
            var shop = await _shopService.GetShopByUserIdAsync(user!.Id);

            if (shop == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var bakiRecords = await _bakiService.GetCustomerBakiAsync(customerId, shop.ShopId);
            var customer = await _customerService.GetCustomerByIdAsync(customerId);

            ViewBag.CustomerName = customer?.FullName;
            return View(bakiRecords);
        }

        public async Task<IActionResult> CreateInvoice()
        {
            var user = await _userManager.GetUserAsync(User);
            var shop = await _shopService.GetShopByUserIdAsync(user!.Id);

            if (shop == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var customers = await _customerService.GetAllCustomersAsync(shop.ShopId);
            ViewBag.Customers = customers;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateInvoice(int customerId, int month, int year)
        {
            var user = await _userManager.GetUserAsync(User);
            var shop = await _shopService.GetShopByUserIdAsync(user!.Id);

            if (shop == null)
            {
                return BadRequest("Shop not found");
            }

            var invoice = await _bakiService.GenerateMonthlyInvoiceAsync(customerId, shop.ShopId, month, year);
            
            TempData["Success"] = "Invoice created successfully!";
            return RedirectToAction(nameof(InvoiceDetails), new { id = invoice.BakiInvoiceId });
        }

        public async Task<IActionResult> InvoiceDetails(int id)
        {
            var invoice = await _bakiService.GetInvoiceDetailsAsync(id);
            
            if (invoice == null)
            {
                TempData["Error"] = "Invoice not found.";
                return RedirectToAction(nameof(Index));
            }
            
            return View(invoice);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendInvoice(int id)
        {
            var result = await _bakiService.SendInvoiceAsync(id);
            if (result)
            {
                TempData["Success"] = "Invoice sent successfully!";
            }
            else
            {
                TempData["Error"] = "Unable to send invoice.";
            }
            return RedirectToAction(nameof(InvoiceDetails), new { id });
        }
    }
}
