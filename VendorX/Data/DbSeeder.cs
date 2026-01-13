using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VendorX.Models;
using VendorX.Models.Enums;

namespace VendorX.Data
{
    public class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Ensure database is created
            await context.Database.MigrateAsync();

            // Seed Roles
            string[] roleNames = { "SuperAdmin", "ShopKeeper", "Customer" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Seed SuperAdmin User
            var superAdminEmail = "admin@vendorx.com";
            var superAdminUser = await userManager.FindByEmailAsync(superAdminEmail);

            if (superAdminUser == null)
            {
                superAdminUser = new ApplicationUser
                {
                    UserName = superAdminEmail,
                    Email = superAdminEmail,
                    FullName = "Super Admin",
                    EmailConfirmed = true,
                    Role = UserRole.SuperAdmin,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                var result = await userManager.CreateAsync(superAdminUser, "Admin@123");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(superAdminUser, "SuperAdmin");
                }
            }

            // Seed Default Expense Categories
            if (!await context.ExpenseCategories.AnyAsync(ec => ec.IsDefault))
            {
                var defaultCategories = new List<ExpenseCategory>
                {
                    new ExpenseCategory
                    {
                        CategoryName = "Electric Bill",
                        Description = "Monthly electricity expenses",
                        Icon = "bi-lightning-charge",
                        Color = "warning",
                        IsDefault = true,
                        IsActive = true,
                        ShopId = null
                    },
                    new ExpenseCategory
                    {
                        CategoryName = "Shop Rent (Monthly)",
                        Description = "Monthly shop rental payment",
                        Icon = "bi-house-door",
                        Color = "primary",
                        IsDefault = true,
                        IsActive = true,
                        ShopId = null
                    },
                    new ExpenseCategory
                    {
                        CategoryName = "Shop Rent (Yearly)",
                        Description = "Yearly shop rental payment",
                        Icon = "bi-house",
                        Color = "info",
                        IsDefault = true,
                        IsActive = true,
                        ShopId = null
                    },
                    new ExpenseCategory
                    {
                        CategoryName = "Water Bill",
                        Description = "Monthly water expenses",
                        Icon = "bi-droplet",
                        Color = "info",
                        IsDefault = true,
                        IsActive = true,
                        ShopId = null
                    },
                    new ExpenseCategory
                    {
                        CategoryName = "Internet Bill",
                        Description = "Monthly internet/WiFi expenses",
                        Icon = "bi-wifi",
                        Color = "primary",
                        IsDefault = true,
                        IsActive = true,
                        ShopId = null
                    },
                    new ExpenseCategory
                    {
                        CategoryName = "Gas Bill",
                        Description = "Monthly gas expenses",
                        Icon = "bi-fire",
                        Color = "danger",
                        IsDefault = true,
                        IsActive = true,
                        ShopId = null
                    },
                    new ExpenseCategory
                    {
                        CategoryName = "Employee Salary",
                        Description = "Monthly employee salaries",
                        Icon = "bi-person-badge",
                        Color = "success",
                        IsDefault = true,
                        IsActive = true,
                        ShopId = null
                    },
                    new ExpenseCategory
                    {
                        CategoryName = "Maintenance & Repairs",
                        Description = "Shop maintenance and repair costs",
                        Icon = "bi-tools",
                        Color = "warning",
                        IsDefault = true,
                        IsActive = true,
                        ShopId = null
                    },
                    new ExpenseCategory
                    {
                        CategoryName = "Office Supplies",
                        Description = "Stationery and office supplies",
                        Icon = "bi-clipboard",
                        Color = "secondary",
                        IsDefault = true,
                        IsActive = true,
                        ShopId = null
                    },
                    new ExpenseCategory
                    {
                        CategoryName = "Marketing & Advertising",
                        Description = "Marketing and promotional expenses",
                        Icon = "bi-megaphone",
                        Color = "primary",
                        IsDefault = true,
                        IsActive = true,
                        ShopId = null
                    },
                    new ExpenseCategory
                    {
                        CategoryName = "Transportation",
                        Description = "Transport and delivery costs",
                        Icon = "bi-truck",
                        Color = "info",
                        IsDefault = true,
                        IsActive = true,
                        ShopId = null
                    },
                    new ExpenseCategory
                    {
                        CategoryName = "Insurance",
                        Description = "Insurance premiums",
                        Icon = "bi-shield-check",
                        Color = "success",
                        IsDefault = true,
                        IsActive = true,
                        ShopId = null
                    },
                    new ExpenseCategory
                    {
                        CategoryName = "License & Permits",
                        Description = "Business licenses and permits",
                        Icon = "bi-file-earmark-text",
                        Color = "warning",
                        IsDefault = true,
                        IsActive = true,
                        ShopId = null
                    },
                    new ExpenseCategory
                    {
                        CategoryName = "Miscellaneous",
                        Description = "Other expenses",
                        Icon = "bi-three-dots",
                        Color = "secondary",
                        IsDefault = true,
                        IsActive = true,
                        ShopId = null
                    }
                };

                await context.ExpenseCategories.AddRangeAsync(defaultCategories);
                await context.SaveChangesAsync();
            }
        }
    }
}
