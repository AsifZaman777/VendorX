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
    public class ShopController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IShopService _shopService;

        public ShopController(UserManager<ApplicationUser> userManager, IShopService shopService)
        {
            _userManager = userManager;
            _shopService = shopService;
        }

        // GET: ShopKeeper/Shop/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            var existingShop = await _shopService.GetShopByUserIdAsync(user!.Id);
            
            if (existingShop != null)
            {
                TempData["Info"] = "You already have a shop. Redirecting to edit.";
                return RedirectToAction(nameof(Edit));
            }

            return View(new ShopViewModel());
        }

        // POST: ShopKeeper/Shop/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ShopViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            var existingShop = await _shopService.GetShopByUserIdAsync(user!.Id);
            
            if (existingShop != null)
            {
                TempData["Error"] = "You already have a shop.";
                return RedirectToAction(nameof(Edit));
            }

            await _shopService.CreateShopAsync(model, user!.Id);
            TempData["Success"] = "Shop created successfully!";
            return RedirectToAction("Index", "Home");
        }

        // GET: ShopKeeper/Shop/Edit
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            var shop = await _shopService.GetShopByUserIdAsync(user!.Id);
            
            if (shop == null)
            {
                TempData["Warning"] = "Please create your shop first.";
                return RedirectToAction(nameof(Create));
            }

            var model = new ShopViewModel
            {
                ShopId = shop.ShopId,
                ShopName = shop.ShopName,
                Address = shop.Address,
                PhoneNumber = shop.PhoneNumber,
                Email = shop.Email,
                QRCode = shop.QRCode,
                ShopCode = shop.ShopCode,
                IsActive = shop.IsActive
            };

            return View(model);
        }

        // POST: ShopKeeper/Shop/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ShopViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            var shop = await _shopService.GetShopByUserIdAsync(user!.Id);
            
            if (shop == null || shop.ShopId != model.ShopId)
            {
                TempData["Error"] = "Unauthorized access.";
                return RedirectToAction("Index", "Home");
            }

            var result = await _shopService.UpdateShopAsync(model);
            
            if (result)
            {
                TempData["Success"] = "Shop updated successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to update shop.";
            }

            return RedirectToAction("Index", "Home");
        }

        // GET: ShopKeeper/Shop/Details
        [HttpGet]
        public async Task<IActionResult> Details()
        {
            var user = await _userManager.GetUserAsync(User);
            var shop = await _shopService.GetShopByUserIdAsync(user!.Id);
            
            if (shop == null)
            {
                TempData["Warning"] = "Please create your shop first.";
                return RedirectToAction(nameof(Create));
            }

            var model = new ShopViewModel
            {
                ShopId = shop.ShopId,
                ShopName = shop.ShopName,
                Address = shop.Address,
                PhoneNumber = shop.PhoneNumber,
                Email = shop.Email,
                QRCode = shop.QRCode,
                ShopCode = shop.ShopCode,
                IsActive = shop.IsActive
            };

            return View(model);
        }
    }
}
