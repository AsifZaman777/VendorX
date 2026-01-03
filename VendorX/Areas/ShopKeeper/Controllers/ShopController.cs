using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
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
        private readonly IConfiguration _configuration;

        public ShopController(
            UserManager<ApplicationUser> userManager, 
            IShopService shopService,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _shopService = shopService;
            _configuration = configuration;
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

            // Get base URL from configuration or request
            var baseUrl = _configuration["ApplicationSettings:BaseUrl"] 
                ?? $"{Request.Scheme}://{Request.Host}";
            
            await _shopService.CreateShopAsync(model, user!.Id, baseUrl);
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

        // POST: ShopKeeper/Shop/RegenerateQRCode
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegenerateQRCode()
        {
            var user = await _userManager.GetUserAsync(User);
            var shop = await _shopService.GetShopByUserIdAsync(user!.Id);
            
            if (shop == null)
            {
                TempData["Error"] = "Shop not found.";
                return RedirectToAction("Index", "Home");
            }

            // Get base URL from configuration or request
            var baseUrl = _configuration["ApplicationSettings:BaseUrl"] 
                ?? $"{Request.Scheme}://{Request.Host}";
            
            var result = await _shopService.RegenerateQRCodeAsync(shop.ShopId, baseUrl);
            
            if (result)
            {
                TempData["Success"] = "QR Code regenerated successfully with updated URL!";
            }
            else
            {
                TempData["Error"] = "Failed to regenerate QR Code.";
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
