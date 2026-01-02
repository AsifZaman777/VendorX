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
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IShopService _shopService;

        public ProductsController(
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

            var products = await _context.Products
                .Where(p => p.ShopId == shop.ShopId)
                .Include(p => p.Category)
                .OrderBy(p => p.ProductName)
                .ToListAsync();

            return View(products);
        }

        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            var shop = await _shopService.GetShopByUserIdAsync(user!.Id);

            if (shop == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var categories = await _context.Categories
                .Where(c => c.ShopId == shop.ShopId && c.IsActive)
                .ToListAsync();

            ViewBag.Categories = categories;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                var shop = await _shopService.GetShopByUserIdAsync(user!.Id);

                if (shop == null)
                {
                    return BadRequest("Shop not found");
                }

                var product = new Product
                {
                    ProductName = model.ProductName,
                    Description = model.Description,
                    Price = model.Price,
                    StockQuantity = model.StockQuantity,
                    CategoryId = model.CategoryId,
                    ShopId = shop.ShopId,
                    ImageUrl = model.ImageUrl,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Product created successfully!";
                return RedirectToAction(nameof(Index));
            }

            var user2 = await _userManager.GetUserAsync(User);
            var shop2 = await _shopService.GetShopByUserIdAsync(user2!.Id);
            var categories = await _context.Categories
                .Where(c => c.ShopId == shop2!.ShopId && c.IsActive)
                .ToListAsync();
            ViewBag.Categories = categories;

            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            var shop = await _shopService.GetShopByUserIdAsync(user!.Id);

            var categories = await _context.Categories
                .Where(c => c.ShopId == shop!.ShopId && c.IsActive)
                .ToListAsync();

            ViewBag.Categories = categories;

            var model = new ProductViewModel
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                CategoryId = product.CategoryId,
                ImageUrl = product.ImageUrl,
                IsActive = product.IsActive
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                var product = await _context.Products.FindAsync(model.ProductId);
                if (product == null)
                {
                    return NotFound();
                }

                product.ProductName = model.ProductName;
                product.Description = model.Description;
                product.Price = model.Price;
                product.StockQuantity = model.StockQuantity;
                product.CategoryId = model.CategoryId;
                product.ImageUrl = model.ImageUrl;
                product.IsActive = model.IsActive;

                await _context.SaveChangesAsync();

                TempData["Success"] = "Product updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.GetUserAsync(User);
            var shop = await _shopService.GetShopByUserIdAsync(user!.Id);
            var categories = await _context.Categories
                .Where(c => c.ShopId == shop!.ShopId && c.IsActive)
                .ToListAsync();
            ViewBag.Categories = categories;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                product.IsActive = false;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Product deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Product not found.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
