using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using VendorX.Services;
using VendorX.ViewModels;

namespace VendorX.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin")]
    [Authorize(Roles = "SuperAdmin")]
    public class ShopsController : Controller
    {
        private readonly IShopService _shopService;
        private readonly IConfiguration _configuration;

        public ShopsController(IShopService shopService, IConfiguration configuration)
        {
            _shopService = shopService;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            var shops = await _shopService.GetAllShopsAsync();
            return View(shops);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ShopViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Get base URL from configuration or request
                var baseUrl = _configuration["ApplicationSettings:BaseUrl"] 
                    ?? $"{Request.Scheme}://{Request.Host}";
                
                await _shopService.CreateShopAsync(model, null, baseUrl);
                TempData["Success"] = "Shop created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var shop = await _shopService.GetShopByIdAsync(id);
            if (shop == null)
            {
                return NotFound();
            }
            return View(shop);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ShopViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _shopService.UpdateShopAsync(model);
                if (result)
                {
                    TempData["Success"] = "Shop updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Unable to update shop.");
            }
            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var shop = await _shopService.GetShopByIdAsync(id);
            if (shop == null)
            {
                return NotFound();
            }
            return View(shop);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _shopService.DeleteShopAsync(id);
            if (result)
            {
                TempData["Success"] = "Shop deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Unable to delete shop.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
