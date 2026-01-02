using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VendorX.Models;
using VendorX.Services;
using VendorX.ViewModels;

namespace VendorX.Areas.ShopKeeper.Controllers
{
    [Area("ShopKeeper")]
    [Authorize(Roles = "ShopKeeper")]
    public class CustomersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IShopService _shopService;
        private readonly ICustomerService _customerService;

        public CustomersController(
            UserManager<ApplicationUser> userManager,
            IShopService shopService,
            ICustomerService customerService)
        {
            _userManager = userManager;
            _shopService = shopService;
            _customerService = customerService;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var shop = await _shopService.GetShopByUserIdAsync(user!.Id);

            if (shop == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var customers = await _customerService.GetAllCustomersAsync(shop.ShopId);
            return View(customers);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                var shop = await _shopService.GetShopByUserIdAsync(user!.Id);

                if (shop == null)
                {
                    return BadRequest("Shop not found");
                }

                var customerId = await _customerService.CreateCustomerAsync(model);
                await _customerService.RegisterCustomerToShopAsync(customerId, shop.ShopId);

                TempData["Success"] = "Customer created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CustomerViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _customerService.UpdateCustomerAsync(model);
                if (result)
                {
                    TempData["Success"] = "Customer updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Unable to update customer.");
            }
            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var shop = await _shopService.GetShopByUserIdAsync(user!.Id);

            if (shop == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            customer.TotalBakiAmount = await _customerService.GetCustomerTotalBakiAsync(id, shop.ShopId);
            return View(customer);
        }
    }
}
