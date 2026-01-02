using Microsoft.EntityFrameworkCore;
using VendorX.Models;
using VendorX.ViewModels;

namespace VendorX.Services
{
    public interface ICustomerService
    {
        Task<List<CustomerViewModel>> GetAllCustomersAsync(int shopId);
        Task<CustomerViewModel?> GetCustomerByIdAsync(int customerId);
        Task<int> CreateCustomerAsync(CustomerViewModel model, string? userId = null);
        Task<bool> UpdateCustomerAsync(CustomerViewModel model);
        Task<bool> DeleteCustomerAsync(int customerId);
        Task<bool> RegisterCustomerToShopAsync(int customerId, int shopId);
        Task<decimal> GetCustomerTotalBakiAsync(int customerId, int shopId);
    }

    public class CustomerService : ICustomerService
    {
        private readonly ApplicationDbContext _context;

        public CustomerService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<CustomerViewModel>> GetAllCustomersAsync(int shopId)
        {
            var customers = await _context.ShopCustomers
                .Where(sc => sc.ShopId == shopId)
                .Select(sc => sc.Customer)
                .ToListAsync();

            var viewModels = new List<CustomerViewModel>();
            foreach (var customer in customers)
            {
                var totalBaki = await GetCustomerTotalBakiAsync(customer.CustomerId, shopId);
                viewModels.Add(new CustomerViewModel
                {
                    CustomerId = customer.CustomerId,
                    FullName = customer.FullName,
                    PhoneNumber = customer.PhoneNumber,
                    Email = customer.Email,
                    Address = customer.Address,
                    IsActive = customer.IsActive,
                    TotalBakiAmount = totalBaki
                });
            }

            return viewModels;
        }

        public async Task<CustomerViewModel?> GetCustomerByIdAsync(int customerId)
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null) return null;

            return new CustomerViewModel
            {
                CustomerId = customer.CustomerId,
                FullName = customer.FullName,
                PhoneNumber = customer.PhoneNumber,
                Email = customer.Email,
                Address = customer.Address,
                IsActive = customer.IsActive
            };
        }

        public async Task<int> CreateCustomerAsync(CustomerViewModel model, string? userId = null)
        {
            var customer = new Customer
            {
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber,
                Email = model.Email,
                Address = model.Address,
                UserId = userId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            return customer.CustomerId;
        }

        public async Task<bool> UpdateCustomerAsync(CustomerViewModel model)
        {
            var customer = await _context.Customers.FindAsync(model.CustomerId);
            if (customer == null) return false;

            customer.FullName = model.FullName;
            customer.PhoneNumber = model.PhoneNumber;
            customer.Email = model.Email;
            customer.Address = model.Address;
            customer.IsActive = model.IsActive;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteCustomerAsync(int customerId)
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null) return false;

            customer.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RegisterCustomerToShopAsync(int customerId, int shopId)
        {
            var exists = await _context.ShopCustomers
                .AnyAsync(sc => sc.CustomerId == customerId && sc.ShopId == shopId);

            if (exists) return false;

            _context.ShopCustomers.Add(new ShopCustomer
            {
                CustomerId = customerId,
                ShopId = shopId,
                RegisteredAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<decimal> GetCustomerTotalBakiAsync(int customerId, int shopId)
        {
            return await _context.BakiRecords
                .Where(b => b.CustomerId == customerId && b.ShopId == shopId && b.Status == Models.Enums.BakiStatus.Due)
                .SumAsync(b => b.Amount);
        }
    }
}
