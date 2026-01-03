using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using VendorX.Data;
using VendorX.Models;
using VendorX.Models.Enums;
using VendorX.ViewModels;
using VendorX.Services;

namespace VendorX.Controllers
{
    public class QRRegistrationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ICustomerService _customerService;

        public QRRegistrationController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ICustomerService customerService)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _customerService = customerService;
        }

        // GET: /QRRegistration/Register?shopCode=SHOP123
        [HttpGet]
        public async Task<IActionResult> Register(string shopCode)
        {
            if (string.IsNullOrEmpty(shopCode))
            {
                TempData["Error"] = "Invalid shop code.";
                return RedirectToAction("Index", "Home");
            }

            var shop = await _context.Shops
                .FirstOrDefaultAsync(s => s.ShopCode == shopCode && s.IsActive);

            if (shop == null)
            {
                TempData["Error"] = "Shop not found or inactive.";
                return RedirectToAction("Index", "Home");
            }

            var model = new QRCustomerRegistrationViewModel
            {
                ShopId = shop.ShopId,
                ShopCode = shopCode,
                ShopName = shop.ShopName,
                ShopAddress = shop.Address,
                ShopPhone = shop.PhoneNumber
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(QRCustomerRegistrationViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if shop exists
                var shop = await _context.Shops
                    .FirstOrDefaultAsync(s => s.ShopId == model.ShopId && s.IsActive);

                if (shop == null)
                {
                    TempData["Error"] = "Shop not found.";
                    return View(model);
                }

                // Check if user with email or phone already exists
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                
                if (existingUser != null)
                {
                    // User exists, just link to shop
                    var existingCustomer = await _context.Customers
                        .FirstOrDefaultAsync(c => c.UserId == existingUser.Id);

                    if (existingCustomer != null)
                    {
                        // Check if already registered to this shop
                        var alreadyRegistered = await _context.ShopCustomers
                            .AnyAsync(sc => sc.ShopId == shop.ShopId && sc.CustomerId == existingCustomer.CustomerId);

                        if (!alreadyRegistered)
                        {
                            // Link customer to shop
                            var shopCustomer = new ShopCustomer
                            {
                                ShopId = shop.ShopId,
                                CustomerId = existingCustomer.CustomerId,
                                RegisteredAt = DateTime.UtcNow
                            };
                            _context.ShopCustomers.Add(shopCustomer);
                            await _context.SaveChangesAsync();

                            TempData["Success"] = $"Successfully registered to {shop.ShopName}!";
                        }
                        else
                        {
                            TempData["Info"] = "You are already registered to this shop.";
                        }

                        // Sign in the user
                        await _signInManager.SignInAsync(existingUser, isPersistent: false);
                        return RedirectToAction("Index", "Home", new { area = "Customer" });
                    }
                }

                // Create new user and customer
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    PhoneNumber = model.PhoneNumber,
                    Role = UserRole.Customer,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Add user to Customer role
                    await _userManager.AddToRoleAsync(user, "Customer");

                    // Create customer record
                    var customer = new Customer
                    {
                        UserId = user.Id,
                        FullName = model.FullName,
                        Email = model.Email,
                        PhoneNumber = model.PhoneNumber,
                        Address = model.Address,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    };
                    _context.Customers.Add(customer);
                    await _context.SaveChangesAsync();

                    // Link customer to shop
                    var shopCustomer = new ShopCustomer
                    {
                        ShopId = shop.ShopId,
                        CustomerId = customer.CustomerId,
                        RegisteredAt = DateTime.UtcNow
                    };
                    _context.ShopCustomers.Add(shopCustomer);
                    await _context.SaveChangesAsync();

                    // Sign in the user
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    TempData["Success"] = $"Successfully registered to {shop.ShopName}! Welcome!";
                    return RedirectToAction("Index", "Home", new { area = "Customer" });
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // Reload shop information for view
            var shopInfo = await _context.Shops
                .FirstOrDefaultAsync(s => s.ShopId == model.ShopId);
            
            if (shopInfo != null)
            {
                model.ShopName = shopInfo.ShopName;
                model.ShopAddress = shopInfo.Address;
                model.ShopPhone = shopInfo.PhoneNumber;
            }

            return View(model);
        }
    }
}
