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

            // Check if user is already logged in
            if (User.Identity?.IsAuthenticated == true)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                
                if (currentUser != null && User.IsInRole("Customer"))
                {
                    // Get customer record
                    var customer = await _context.Customers
                        .FirstOrDefaultAsync(c => c.UserId == currentUser.Id);

                    if (customer != null)
                    {
                        // Check if already registered to this shop
                        var alreadyRegistered = await _context.ShopCustomers
                            .AnyAsync(sc => sc.ShopId == shop.ShopId && sc.CustomerId == customer.CustomerId);

                        if (alreadyRegistered)
                        {
                            TempData["Info"] = $"You are already registered to {shop.ShopName}. Redirecting to dashboard...";
                            return RedirectToAction("Index", "Home", new { area = "Customer" });
                        }
                        else
                        {
                            // Show confirmation page for linking
                            var linkModel = new QRCustomerRegistrationViewModel
                            {
                                ShopId = shop.ShopId,
                                ShopCode = shopCode,
                                ShopName = shop.ShopName,
                                ShopAddress = shop.Address,
                                ShopPhone = shop.PhoneNumber,
                                FullName = customer.FullName,
                                Email = customer.Email,
                                PhoneNumber = customer.PhoneNumber,
                                Address = customer.Address,
                                IsExistingCustomer = true
                            };
                            return View("LinkShop", linkModel);
                        }
                    }
                }
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

                        // Sign in the user with persistent cookie (7 days)
                        await _signInManager.SignInAsync(existingUser, isPersistent: true);
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

                    // Sign in the user with persistent cookie (7 days)
                    await _signInManager.SignInAsync(user, isPersistent: true);

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

        // POST: /QRRegistration/LinkShop
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LinkShop(int shopId)
        {
            if (!User.Identity?.IsAuthenticated == true)
            {
                TempData["Error"] = "You must be logged in to link to a shop.";
                return RedirectToAction("Login", "Account");
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("Index", "Home");
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.UserId == currentUser.Id);

            if (customer == null)
            {
                TempData["Error"] = "Customer record not found.";
                return RedirectToAction("Index", "Home");
            }

            var shop = await _context.Shops
                .FirstOrDefaultAsync(s => s.ShopId == shopId && s.IsActive);

            if (shop == null)
            {
                TempData["Error"] = "Shop not found.";
                return RedirectToAction("Index", "Home");
            }

            // Check if already linked
            var existingLink = await _context.ShopCustomers
                .FirstOrDefaultAsync(sc => sc.ShopId == shopId && sc.CustomerId == customer.CustomerId);

            if (existingLink != null)
            {
                TempData["Info"] = $"You are already registered to {shop.ShopName}.";
                return RedirectToAction("Index", "Home", new { area = "Customer" });
            }

            // Create link
            var shopCustomer = new ShopCustomer
            {
                ShopId = shopId,
                CustomerId = customer.CustomerId,
                RegisteredAt = DateTime.UtcNow
            };

            _context.ShopCustomers.Add(shopCustomer);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Successfully registered to {shop.ShopName}!";
            return RedirectToAction("Index", "Home", new { area = "Customer" });
        }
    }
}
