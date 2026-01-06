using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VendorX.Data;
using VendorX.Models;
using VendorX.Models.Enums;
using VendorX.Services;
using VendorX.ViewModels;

namespace VendorX.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ICustomerService _customerService;
        private readonly ApplicationDbContext _context;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ICustomerService customerService,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _customerService = customerService;
            _context = context;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                // Set isPersistent to true when RememberMe is checked, false otherwise
                var result = await _signInManager.PasswordSignInAsync(
                    model.Email, 
                    model.Password, 
                    isPersistent: model.RememberMe,
                    lockoutOnFailure: false);
                
                if (result.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    
                    if (user != null)
                    {
                        // Check if user account is active
                        if (!user.IsActive)
                        {
                            await _signInManager.SignOutAsync();
                            ModelState.AddModelError(string.Empty, "Your account has been deactivated. Please contact support.");
                            return View(model);
                        }

                        // For ShopKeepers, check if their shop is active
                        if (user.Role == UserRole.ShopKeeper)
                        {
                            var shop = await _context.Shops
                                .FirstOrDefaultAsync(s => s.UserId == user.Id);
                            
                            if (shop != null && !shop.IsActive)
                            {
                                await _signInManager.SignOutAsync();
                                ModelState.AddModelError(string.Empty, 
                                    "Your shop subscription is inactive. Please renew your subscription to continue. Contact admin for assistance.");
                                return View(model);
                            }
                            
                            return RedirectToAction("Index", "Home", new { area = "ShopKeeper" });
                        }
                        else if (user.Role == UserRole.SuperAdmin)
                        {
                            return RedirectToAction("Index", "Home", new { area = "SuperAdmin" });
                        }
                        else if (user.Role == UserRole.Customer)
                        {
                            return RedirectToAction("Index", "Home", new { area = "Customer" });
                        }
                    }

                    return RedirectToLocal(returnUrl);
                }
                
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    PhoneNumber = model.PhoneNumber,
                    Role = model.Role == "ShopKeeper" ? UserRole.ShopKeeper : UserRole.Customer,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Add user to role
                    await _userManager.AddToRoleAsync(user, user.Role.ToString());

                    // If customer, create customer record
                    if (user.Role == UserRole.Customer)
                    {
                        var customerModel = new CustomerViewModel
                        {
                            FullName = model.FullName,
                            Email = model.Email,
                            PhoneNumber = model.PhoneNumber
                        };
                        await _customerService.CreateCustomerAsync(customerModel, user.Id);
                    }

                    await _signInManager.SignInAsync(user, isPersistent: false);

                    // Redirect based on role
                    if (user.Role == UserRole.ShopKeeper)
                    {
                        return RedirectToAction("Index", "Home", new { area = "ShopKeeper" });
                    }
                    else if (user.Role == UserRole.Customer)
                    {
                        return RedirectToAction("Index", "Home", new { area = "Customer" });
                    }

                    return RedirectToAction(nameof(HomeController.Index), "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }
    }
}
