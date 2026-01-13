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
    public class ExpensesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IShopService _shopService;

        public ExpensesController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IShopService shopService)
        {
            _context = context;
            _userManager = userManager;
            _shopService = shopService;
        }

        // GET: Expenses
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var shop = await _shopService.GetShopByUserIdAsync(user!.Id);

            if (shop == null)
            {
                return RedirectToAction("Index", "Home");
            }

            // Get all categories (default + shop-specific)
            var categories = await _context.ExpenseCategories
                .Where(ec => ec.IsDefault || ec.ShopId == shop.ShopId)
                .Where(ec => ec.IsActive)
                .Select(ec => new ExpenseCategoryViewModel
                {
                    ExpenseCategoryId = ec.ExpenseCategoryId,
                    CategoryName = ec.CategoryName,
                    Description = ec.Description,
                    Icon = ec.Icon,
                    Color = ec.Color,
                    TotalAmount = ec.Expenses.Where(e => e.ShopId == shop.ShopId).Sum(e => e.Amount),
                    ExpenseCount = ec.Expenses.Count(e => e.ShopId == shop.ShopId),
                    PendingCount = ec.Expenses.Count(e => e.ShopId == shop.ShopId && e.Status == "Pending"),
                    PendingAmount = ec.Expenses.Where(e => e.ShopId == shop.ShopId && e.Status == "Pending").Sum(e => e.Amount)
                })
                .ToListAsync();

            ViewBag.TotalExpenses = categories.Sum(c => c.TotalAmount);
            ViewBag.TotalPending = categories.Sum(c => c.PendingAmount);
            ViewBag.TotalPaid = ViewBag.TotalExpenses - ViewBag.TotalPending;
            ViewBag.ShopId = shop.ShopId;

            return View(categories);
        }

        // GET: GetExpensesByCategory
        [HttpGet]
        public async Task<IActionResult> GetExpensesByCategory(int categoryId)
        {
            var user = await _userManager.GetUserAsync(User);
            var shop = await _shopService.GetShopByUserIdAsync(user!.Id);

            if (shop == null)
            {
                return Json(new { success = false, message = "Shop not found" });
            }

            var expenses = await _context.Expenses
                .Where(e => e.ShopId == shop.ShopId && e.ExpenseCategoryId == categoryId)
                .OrderByDescending(e => e.ExpenseDate)
                .Select(e => new
                {
                    e.ExpenseId,
                    Title = e.ExpenseName,
                    e.Description,
                    e.Amount,
                    ExpenseDate = e.ExpenseDate.ToString("MMM dd, yyyy"),
                    DueDate = e.DueDate.HasValue ? e.DueDate.Value.ToString("MMM dd, yyyy") : null,
                    e.Status,
                    e.PaymentMethod,
                    e.ReceiptNumber,
                    e.Vendor,
                    e.Notes,
                    IsFromFixedExpense = e.FixedExpenseId.HasValue
                })
                .ToListAsync();

            return Json(new { success = true, data = expenses });
        }

        // POST: CreateExpense
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateExpense(ExpenseViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            var shop = await _shopService.GetShopByUserIdAsync(user!.Id);

            if (shop == null)
            {
                return Json(new { success = false, message = "Shop not found" });
            }

            model.ShopId = shop.ShopId;

            ModelState.Remove("ExpenseCategoryName");
            ModelState.Remove("PaidAt");

            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Please fill in all required fields" });
            }

            var expense = new Expense
            {
                ShopId = model.ShopId,
                ExpenseCategoryId = model.ExpenseCategoryId,
                ExpenseName = model.Title,
                Description = model.Description,
                Amount = model.Amount,
                ExpenseDate = model.ExpenseDate,
                DueDate = model.DueDate,
                Status = model.Status,
                PaymentMethod = model.PaymentMethod,
                ReceiptNumber = model.ReceiptNumber,
                Vendor = model.Vendor,
                Notes = model.Notes,
                CreatedAt = DateTime.Now
            };

            if (model.Status == "Paid")
            {
                expense.PaidAt = DateTime.Now;
            }

            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Expense added successfully!" });
        }

        // POST: MarkAsPaid
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsPaid(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var shop = await _shopService.GetShopByUserIdAsync(user!.Id);

            if (shop == null)
            {
                return Json(new { success = false, message = "Shop not found" });
            }

            var expense = await _context.Expenses
                .FirstOrDefaultAsync(e => e.ExpenseId == id && e.ShopId == shop.ShopId);

            if (expense == null)
            {
                return Json(new { success = false, message = "Expense not found" });
            }

            expense.Status = "Paid";
            expense.PaidAt = DateTime.Now;
            expense.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Expense marked as paid!" });
        }

        // POST: DeleteExpense
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteExpense(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var shop = await _shopService.GetShopByUserIdAsync(user!.Id);

            if (shop == null)
            {
                return Json(new { success = false, message = "Shop not found" });
            }

            var expense = await _context.Expenses
                .FirstOrDefaultAsync(e => e.ExpenseId == id && e.ShopId == shop.ShopId);

            if (expense == null)
            {
                return Json(new { success = false, message = "Expense not found" });
            }

            // Don't allow deleting auto-generated expenses
            if (expense.FixedExpenseId.HasValue)
            {
                return Json(new { success = false, message = "Cannot delete auto-generated expenses. Disable the fixed expense instead." });
            }

            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Expense deleted successfully!" });
        }

        // GET: Categories
        public async Task<IActionResult> Categories()
        {
            var user = await _userManager.GetUserAsync(User);
            var shop = await _shopService.GetShopByUserIdAsync(user!.Id);

            if (shop == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var categories = await _context.ExpenseCategories
                .Where(ec => ec.ShopId == shop.ShopId)
                .OrderBy(ec => ec.CategoryName)
                .ToListAsync();

            return View(categories);
        }

        // POST: CreateCategory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(ExpenseCategoryViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            var shop = await _shopService.GetShopByUserIdAsync(user!.Id);

            if (shop == null)
            {
                TempData["Error"] = "Shop not found";
                return RedirectToAction(nameof(Categories));
            }

            ModelState.Remove("ExpenseCategoryId");
            ModelState.Remove("TotalAmount");
            ModelState.Remove("ExpenseCount");
            ModelState.Remove("PendingCount");
            ModelState.Remove("PendingAmount");

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please fill in all required fields";
                return RedirectToAction(nameof(Categories));
            }

            var category = new ExpenseCategory
            {
                ShopId = shop.ShopId,
                CategoryName = model.CategoryName,
                Description = model.Description,
                Icon = model.Icon,
                Color = model.Color,
                IsDefault = false,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _context.ExpenseCategories.Add(category);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Category created successfully!";
            return RedirectToAction(nameof(Categories));
        }

        // POST: DeleteCategory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var shop = await _shopService.GetShopByUserIdAsync(user!.Id);

            if (shop == null)
            {
                TempData["Error"] = "Shop not found";
                return RedirectToAction(nameof(Categories));
            }

            var category = await _context.ExpenseCategories
                .Include(ec => ec.Expenses)
                .FirstOrDefaultAsync(ec => ec.ExpenseCategoryId == id && ec.ShopId == shop.ShopId);

            if (category == null)
            {
                TempData["Error"] = "Category not found";
                return RedirectToAction(nameof(Categories));
            }

            if (category.IsDefault)
            {
                TempData["Error"] = "Cannot delete default categories";
                return RedirectToAction(nameof(Categories));
            }

            if (category.Expenses.Any())
            {
                TempData["Error"] = "Cannot delete category with existing expenses";
                return RedirectToAction(nameof(Categories));
            }

            _context.ExpenseCategories.Remove(category);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Category deleted successfully!";
            return RedirectToAction(nameof(Categories));
        }

        // GET: FixedExpenses
        public async Task<IActionResult> FixedExpenses()
        {
            var user = await _userManager.GetUserAsync(User);
            var shop = await _shopService.GetShopByUserIdAsync(user!.Id);

            if (shop == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var fixedExpenses = await _context.FixedExpenses
                .Include(fe => fe.ExpenseCategory)
                .Where(fe => fe.ShopId == shop.ShopId)
                .OrderByDescending(fe => fe.IsActive)
                .ThenBy(fe => fe.NextDueDate)
                .Select(fe => new FixedExpenseViewModel
                {
                    FixedExpenseId = fe.FixedExpenseId,
                    ExpenseName = fe.ExpenseName,
                    Amount = fe.Amount,
                    Description = fe.Description,
                    ExpenseCategoryId = fe.ExpenseCategoryId,
                    ExpenseCategoryName = fe.ExpenseCategory.CategoryName,
                    RecurrenceType = fe.RecurrenceType,
                    RecurrenceInterval = fe.RecurrenceInterval,
                    StartDate = fe.StartDate,
                    EndDate = fe.EndDate,
                    NextDueDate = fe.NextDueDate,
                    IsActive = fe.IsActive,
                    PaymentMethod = fe.PaymentMethod,
                    Vendor = fe.Vendor
                })
                .ToListAsync();

            // Get categories for dropdown
            var categories = await _context.ExpenseCategories
                .Where(ec => ec.IsDefault || ec.ShopId == shop.ShopId)
                .Where(ec => ec.IsActive)
                .ToListAsync();

            ViewBag.Categories = categories;

            return View(fixedExpenses);
        }

        // POST: CreateFixedExpense
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFixedExpense(FixedExpenseViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            var shop = await _shopService.GetShopByUserIdAsync(user!.Id);

            if (shop == null)
            {
                TempData["Error"] = "Shop not found";
                return RedirectToAction(nameof(FixedExpenses));
            }

            model.ShopId = shop.ShopId;

            ModelState.Remove("FixedExpenseId");
            ModelState.Remove("ExpenseCategoryName");
            ModelState.Remove("NextDueDate");

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please fill in all required fields";
                return RedirectToAction(nameof(FixedExpenses));
            }

            var fixedExpense = new FixedExpense
            {
                ShopId = model.ShopId,
                ExpenseCategoryId = model.ExpenseCategoryId,
                ExpenseName = model.ExpenseName,
                Amount = model.Amount,
                Description = model.Description,
                RecurrenceType = model.RecurrenceType,
                RecurrenceInterval = model.RecurrenceInterval,
                DayOfMonth = model.DayOfMonth,
                DayOfWeek = model.DayOfWeek,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                IsActive = true,
                PaymentMethod = model.PaymentMethod,
                Vendor = model.Vendor,
                Notes = model.Notes,
                CreatedAt = DateTime.Now
            };

            // Calculate next due date
            fixedExpense.NextDueDate = CalculateNextDueDate(fixedExpense);

            _context.FixedExpenses.Add(fixedExpense);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Fixed expense created successfully!";
            return RedirectToAction(nameof(FixedExpenses));
        }

        // POST: ToggleFixedExpense
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFixedExpense(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var shop = await _shopService.GetShopByUserIdAsync(user!.Id);

            if (shop == null)
            {
                return Json(new { success = false, message = "Shop not found" });
            }

            var fixedExpense = await _context.FixedExpenses
                .FirstOrDefaultAsync(fe => fe.FixedExpenseId == id && fe.ShopId == shop.ShopId);

            if (fixedExpense == null)
            {
                return Json(new { success = false, message = "Fixed expense not found" });
            }

            fixedExpense.IsActive = !fixedExpense.IsActive;
            fixedExpense.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Json(new { 
                success = true, 
                message = fixedExpense.IsActive ? "Fixed expense activated" : "Fixed expense deactivated",
                isActive = fixedExpense.IsActive
            });
        }

        // Helper method to calculate next due date
        private DateTime CalculateNextDueDate(FixedExpense fixedExpense)
        {
            var nextDate = fixedExpense.StartDate;

            switch (fixedExpense.RecurrenceType.ToLower())
            {
                case "daily":
                    while (nextDate <= DateTime.Today)
                    {
                        nextDate = nextDate.AddDays(fixedExpense.RecurrenceInterval);
                    }
                    break;

                case "weekly":
                    while (nextDate <= DateTime.Today)
                    {
                        nextDate = nextDate.AddDays(7 * fixedExpense.RecurrenceInterval);
                    }
                    break;

                case "monthly":
                    while (nextDate <= DateTime.Today)
                    {
                        nextDate = nextDate.AddMonths(fixedExpense.RecurrenceInterval);
                    }
                    if (fixedExpense.DayOfMonth.HasValue)
                    {
                        var day = Math.Min(fixedExpense.DayOfMonth.Value, DateTime.DaysInMonth(nextDate.Year, nextDate.Month));
                        nextDate = new DateTime(nextDate.Year, nextDate.Month, day);
                    }
                    break;

                case "yearly":
                    while (nextDate <= DateTime.Today)
                    {
                        nextDate = nextDate.AddYears(fixedExpense.RecurrenceInterval);
                    }
                    break;
            }

            return nextDate;
        }
    }
}
