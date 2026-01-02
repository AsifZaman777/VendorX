using Microsoft.EntityFrameworkCore;
using VendorX.Models;
using VendorX.Models.Enums;
using VendorX.ViewModels;

namespace VendorX.Services
{
    public interface IBakiService
    {
        Task<List<BakiViewModel>> GetAllBakiAsync(int shopId, BakiStatus? status = null);
        Task<List<BakiViewModel>> GetCustomerBakiAsync(int customerId, int shopId);
        Task<int> CreateBakiAsync(BakiViewModel model, int shopId);
        Task<bool> SettleBakiAsync(int bakiId);
        Task<BakiInvoiceViewModel> GenerateMonthlyInvoiceAsync(int customerId, int shopId, int month, int year);
        Task<BakiInvoice?> GetInvoiceDetailsAsync(int invoiceId);
        Task<bool> SendInvoiceAsync(int invoiceId);
    }

    public class BakiService : IBakiService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IWhatsAppService _whatsAppService;

        public BakiService(ApplicationDbContext context, IEmailService emailService, IWhatsAppService whatsAppService)
        {
            _context = context;
            _emailService = emailService;
            _whatsAppService = whatsAppService;
        }

        public async Task<List<BakiViewModel>> GetAllBakiAsync(int shopId, BakiStatus? status = null)
        {
            var query = _context.BakiRecords
                .Include(b => b.Customer)
                .Where(b => b.ShopId == shopId);

            if (status.HasValue)
            {
                query = query.Where(b => b.Status == status.Value);
            }

            return await query
                .Select(b => new BakiViewModel
                {
                    BakiId = b.BakiId,
                    CustomerId = b.CustomerId,
                    CustomerName = b.Customer.FullName,
                    Amount = b.Amount,
                    Status = b.Status,
                    CreatedAt = b.CreatedAt,
                    SettledAt = b.SettledAt,
                    Description = b.Description,
                    TransactionType = b.TransactionType
                })
                .ToListAsync();
        }

        public async Task<List<BakiViewModel>> GetCustomerBakiAsync(int customerId, int shopId)
        {
            return await _context.BakiRecords
                .Where(b => b.CustomerId == customerId && b.ShopId == shopId)
                .Select(b => new BakiViewModel
                {
                    BakiId = b.BakiId,
                    CustomerId = b.CustomerId,
                    CustomerName = b.Customer.FullName,
                    Amount = b.Amount,
                    Status = b.Status,
                    CreatedAt = b.CreatedAt,
                    SettledAt = b.SettledAt,
                    Description = b.Description,
                    TransactionType = b.TransactionType
                })
                .ToListAsync();
        }

        public async Task<int> CreateBakiAsync(BakiViewModel model, int shopId)
        {
            var baki = new Baki
            {
                CustomerId = model.CustomerId,
                ShopId = shopId,
                Amount = model.Amount,
                Status = BakiStatus.Due,
                Description = model.Description,
                TransactionType = model.TransactionType,
                CreatedAt = DateTime.UtcNow
            };

            _context.BakiRecords.Add(baki);
            await _context.SaveChangesAsync();

            // Send notification to customer
            var customer = await _context.Customers.FindAsync(model.CustomerId);
            if (customer != null)
            {
                var message = $"New Baki record created. Amount: {model.Amount:C}. Description: {model.Description}";
                
                if (!string.IsNullOrEmpty(customer.Email))
                {
                    await _emailService.SendEmailAsync(customer.Email, "New Baki Record", message);
                }

                if (!string.IsNullOrEmpty(customer.PhoneNumber))
                {
                    await _whatsAppService.SendWhatsAppMessageAsync(customer.PhoneNumber, message);
                }
            }

            return baki.BakiId;
        }

        public async Task<bool> SettleBakiAsync(int bakiId)
        {
            var baki = await _context.BakiRecords.FindAsync(bakiId);
            if (baki == null) return false;

            baki.Status = BakiStatus.Settled;
            baki.SettledAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Send notification to customer
            var customer = await _context.Customers.FindAsync(baki.CustomerId);
            if (customer != null)
            {
                var message = $"Baki settled. Amount: {baki.Amount:C}";
                
                if (!string.IsNullOrEmpty(customer.Email))
                {
                    await _emailService.SendEmailAsync(customer.Email, "Baki Settled", message);
                }

                if (!string.IsNullOrEmpty(customer.PhoneNumber))
                {
                    await _whatsAppService.SendWhatsAppMessageAsync(customer.PhoneNumber, message);
                }
            }

            return true;
        }

        public async Task<BakiInvoiceViewModel> GenerateMonthlyInvoiceAsync(int customerId, int shopId, int month, int year)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var bakiRecords = await _context.BakiRecords
                .Where(b => b.CustomerId == customerId && 
                           b.ShopId == shopId && 
                           b.CreatedAt >= startDate && 
                           b.CreatedAt <= endDate)
                .ToListAsync();

            var customer = await _context.Customers.FindAsync(customerId);

            var invoice = new BakiInvoice
            {
                InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMddHHmmss}",
                CustomerId = customerId,
                ShopId = shopId,
                Month = month,
                Year = year,
                TotalAmount = bakiRecords.Sum(b => b.Amount),
                InvoiceDate = DateTime.UtcNow
            };

            _context.BakiInvoices.Add(invoice);
            await _context.SaveChangesAsync();

            foreach (var baki in bakiRecords)
            {
                var item = new BakiInvoiceItem
                {
                    BakiInvoiceId = invoice.BakiInvoiceId,
                    BakiId = baki.BakiId,
                    Amount = baki.Amount,
                    Description = baki.Description,
                    TransactionDate = baki.CreatedAt
                };
                _context.BakiInvoiceItems.Add(item);
            }

            await _context.SaveChangesAsync();

            return new BakiInvoiceViewModel
            {
                BakiInvoiceId = invoice.BakiInvoiceId,
                InvoiceNumber = invoice.InvoiceNumber,
                CustomerId = customerId,
                CustomerName = customer?.FullName,
                Month = month,
                Year = year,
                TotalAmount = invoice.TotalAmount,
                IsSent = false,
                Items = bakiRecords.Select(b => new BakiInvoiceItemViewModel
                {
                    Amount = b.Amount,
                    Description = b.Description,
                    TransactionDate = b.CreatedAt
                }).ToList()
            };
        }

        public async Task<BakiInvoice?> GetInvoiceDetailsAsync(int invoiceId)
        {
            return await _context.BakiInvoices
                .Include(i => i.Customer)
                .Include(i => i.Shop)
                .Include(i => i.BakiInvoiceItems)
                    .ThenInclude(item => item.Baki)
                .FirstOrDefaultAsync(i => i.BakiInvoiceId == invoiceId);
        }

        public async Task<bool> SendInvoiceAsync(int invoiceId)
        {
            var invoice = await _context.BakiInvoices
                .Include(i => i.Customer)
                .Include(i => i.BakiInvoiceItems)
                .FirstOrDefaultAsync(i => i.BakiInvoiceId == invoiceId);

            if (invoice == null) return false;

            var customer = invoice.Customer;
            var message = $"Monthly Baki Invoice\n" +
                         $"Invoice #: {invoice.InvoiceNumber}\n" +
                         $"Period: {invoice.Month}/{invoice.Year}\n" +
                         $"Total Amount: {invoice.TotalAmount:C}\n" +
                         $"Items: {invoice.BakiInvoiceItems.Count}";

            var emailSent = false;
            var whatsAppSent = false;

            if (!string.IsNullOrEmpty(customer.Email))
            {
                emailSent = await _emailService.SendEmailAsync(customer.Email, "Monthly Baki Invoice", message);
            }

            if (!string.IsNullOrEmpty(customer.PhoneNumber))
            {
                whatsAppSent = await _whatsAppService.SendWhatsAppMessageAsync(customer.PhoneNumber, message);
            }

            if (emailSent || whatsAppSent)
            {
                invoice.IsSent = true;
                invoice.SentAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }
    }
}
