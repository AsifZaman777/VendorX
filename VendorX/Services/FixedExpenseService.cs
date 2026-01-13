using Microsoft.EntityFrameworkCore;
using VendorX.Models;

namespace VendorX.Services
{
    public interface IFixedExpenseService
    {
        Task GenerateDueExpensesAsync();
    }

    public class FixedExpenseService : IFixedExpenseService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<FixedExpenseService> _logger;

        public FixedExpenseService(IServiceScopeFactory scopeFactory, ILogger<FixedExpenseService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task GenerateDueExpensesAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            try
            {
                var today = DateTime.Today;

                // Get all active fixed expenses that are due
                var dueFixedExpenses = await context.FixedExpenses
                    .Include(fe => fe.ExpenseCategory)
                    .Where(fe => fe.IsActive && 
                                 fe.NextDueDate.HasValue && 
                                 fe.NextDueDate.Value <= today &&
                                 (!fe.EndDate.HasValue || fe.EndDate.Value >= today))
                    .ToListAsync();

                _logger.LogInformation($"Found {dueFixedExpenses.Count} fixed expenses due for generation");

                foreach (var fixedExpense in dueFixedExpenses)
                {
                    try
                    {
                        // Check if expense already generated for this due date
                        var alreadyGenerated = await context.Expenses
                            .AnyAsync(e => e.FixedExpenseId == fixedExpense.FixedExpenseId &&
                                          e.ExpenseDate.Date == fixedExpense.NextDueDate.Value.Date);

                        if (alreadyGenerated)
                        {
                            _logger.LogInformation($"Expense already generated for FixedExpense ID: {fixedExpense.FixedExpenseId}");
                            
                            // Update next due date
                            fixedExpense.LastGenerated = fixedExpense.NextDueDate;
                            fixedExpense.NextDueDate = CalculateNextDueDate(fixedExpense);
                            fixedExpense.UpdatedAt = DateTime.Now;
                            continue;
                        }

                        // Generate new expense
                        var expense = new Expense
                        {
                            ShopId = fixedExpense.ShopId,
                            ExpenseCategoryId = fixedExpense.ExpenseCategoryId,
                            ExpenseName = fixedExpense.ExpenseName,
                            Description = fixedExpense.Description,
                            Amount = fixedExpense.Amount,
                            ExpenseDate = fixedExpense.NextDueDate.Value,
                            DueDate = fixedExpense.NextDueDate.Value.AddDays(7), // 7 days to pay
                            Status = "Pending",
                            PaymentMethod = fixedExpense.PaymentMethod,
                            Vendor = fixedExpense.Vendor,
                            Notes = $"Auto-generated from fixed expense: {fixedExpense.ExpenseName}",
                            FixedExpenseId = fixedExpense.FixedExpenseId,
                            CreatedAt = DateTime.Now
                        };

                        context.Expenses.Add(expense);

                        // Update fixed expense
                        fixedExpense.LastGenerated = fixedExpense.NextDueDate;
                        fixedExpense.NextDueDate = CalculateNextDueDate(fixedExpense);
                        fixedExpense.UpdatedAt = DateTime.Now;

                        _logger.LogInformation($"Generated expense for: {fixedExpense.ExpenseName} (Amount: {fixedExpense.Amount})");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error generating expense for FixedExpense ID {fixedExpense.FixedExpenseId}: {ex.Message}");
                    }
                }

                await context.SaveChangesAsync();
                _logger.LogInformation("Fixed expense generation completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GenerateDueExpensesAsync: {ex.Message}");
                throw;
            }
        }

        private DateTime CalculateNextDueDate(FixedExpense fixedExpense)
        {
            var currentDue = fixedExpense.NextDueDate ?? fixedExpense.StartDate;
            DateTime nextDate = currentDue;

            switch (fixedExpense.RecurrenceType.ToLower())
            {
                case "daily":
                    nextDate = currentDue.AddDays(fixedExpense.RecurrenceInterval);
                    break;

                case "weekly":
                    nextDate = currentDue.AddDays(7 * fixedExpense.RecurrenceInterval);
                    break;

                case "monthly":
                    nextDate = currentDue.AddMonths(fixedExpense.RecurrenceInterval);
                    if (fixedExpense.DayOfMonth.HasValue)
                    {
                        var day = Math.Min(fixedExpense.DayOfMonth.Value, DateTime.DaysInMonth(nextDate.Year, nextDate.Month));
                        nextDate = new DateTime(nextDate.Year, nextDate.Month, day);
                    }
                    break;

                case "yearly":
                    nextDate = currentDue.AddYears(fixedExpense.RecurrenceInterval);
                    break;
            }

            // If end date is set and next date exceeds it, return null to stop generation
            if (fixedExpense.EndDate.HasValue && nextDate > fixedExpense.EndDate.Value)
            {
                return DateTime.MaxValue; // Stop generating
            }

            return nextDate;
        }
    }
}
