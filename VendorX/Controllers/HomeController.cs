using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using VendorX.Models;

namespace VendorX.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public HomeController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> Index()
        {
            // Check if user is authenticated
            if (_signInManager.IsSignedIn(User))
            {
                var user = await _userManager.GetUserAsync(User);
                
                if (user != null)
                {
                    // Redirect based on user role
                    if (User.IsInRole("SuperAdmin"))
                    {
                        return RedirectToAction("Index", "Dashboard", new { area = "SuperAdmin" });
                    }
                    else if (User.IsInRole("ShopKeeper"))
                    {
                        return RedirectToAction("Index", "Home", new { area = "ShopKeeper" });
                    }
                    else if (User.IsInRole("Customer"))
                    {
                        return RedirectToAction("Index", "Home", new { area = "Customer" });
                    }
                }
            }

            // If not authenticated, redirect to login
            //return RedirectToAction("Login", "Account");

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
