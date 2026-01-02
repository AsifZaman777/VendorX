using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VendorX.Models;
using VendorX.Services;
using VendorX.ViewModels;

namespace VendorX.Areas.ShopKeeper.Controllers
{
    [Area("ShopKeeper")]
    [Authorize(Roles = "ShopKeeper")]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IShopService _shopService;

        public CategoriesController(
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
                return RedirectToAction("Index", "Home");
            }

            var categories = await _context.Categories
                .Where(c => c.ShopId == shop.ShopId)
                .OrderBy(c => c.CategoryName)
                .ToListAsync();

            return View(categories);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                var shop = await _shopService.GetShopByUserIdAsync(user!.Id);

                if (shop == null)
                {
                    return BadRequest("Shop not found");
                }

                var category = new Category
                {
                    CategoryName = model.CategoryName,
                    Description = model.Description,
                    ShopId = shop.ShopId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Category created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            var model = new CategoryViewModel
            {
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName,
                Description = category.Description,
                IsActive = category.IsActive
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var category = await _context.Categories.FindAsync(model.CategoryId);
                if (category == null)
                {
                    return NotFound();
                }

                category.CategoryName = model.CategoryName;
                category.Description = model.Description;
                category.IsActive = model.IsActive;

                await _context.SaveChangesAsync();

                TempData["Success"] = "Category updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                category.IsActive = false;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Category deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Category not found.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
