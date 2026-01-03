using Microsoft.EntityFrameworkCore;
using VendorX.Models;
using VendorX.Models.Enums;
using VendorX.ViewModels;

namespace VendorX.Services
{
    public interface IShopService
    {
        Task<List<ShopViewModel>> GetAllShopsAsync();
        Task<ShopViewModel?> GetShopByIdAsync(int shopId);
        Task<Shop?> GetShopByUserIdAsync(string userId);
        Task<int> CreateShopAsync(ShopViewModel model, string userId, string baseUrl);
        Task<bool> UpdateShopAsync(ShopViewModel model);
        Task<bool> DeleteShopAsync(int shopId);
        Task<bool> RegenerateQRCodeAsync(int shopId, string baseUrl);
    }

    public class ShopService : IShopService
    {
        private readonly ApplicationDbContext _context;
        private readonly IQRCodeService _qrCodeService;

        public ShopService(ApplicationDbContext context, IQRCodeService qrCodeService)
        {
            _context = context;
            _qrCodeService = qrCodeService;
        }

        public async Task<List<ShopViewModel>> GetAllShopsAsync()
        {
            return await _context.Shops
                .Select(s => new ShopViewModel
                {
                    ShopId = s.ShopId,
                    ShopName = s.ShopName,
                    Address = s.Address,
                    PhoneNumber = s.PhoneNumber,
                    Email = s.Email,
                    QRCode = s.QRCode,
                    ShopCode = s.ShopCode,
                    IsActive = s.IsActive
                })
                .ToListAsync();
        }

        public async Task<ShopViewModel?> GetShopByIdAsync(int shopId)
        {
            var shop = await _context.Shops.FindAsync(shopId);
            if (shop == null) return null;

            return new ShopViewModel
            {
                ShopId = shop.ShopId,
                ShopName = shop.ShopName,
                Address = shop.Address,
                PhoneNumber = shop.PhoneNumber,
                Email = shop.Email,
                QRCode = shop.QRCode,
                ShopCode = shop.ShopCode,
                IsActive = shop.IsActive
            };
        }

        public async Task<Shop?> GetShopByUserIdAsync(string userId)
        {
            return await _context.Shops.FirstOrDefaultAsync(s => s.UserId == userId);
        }

        public async Task<int> CreateShopAsync(ShopViewModel model, string userId, string baseUrl)
        {
            var shopCode = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
            
            // Generate QR code with registration URL using provided base URL
            var qrContent = $"{baseUrl}/QRRegistration/Register?shopCode={shopCode}";
            var qrCode = _qrCodeService.GenerateQRCode(qrContent);

            var shop = new Shop
            {
                ShopName = model.ShopName,
                Address = model.Address,
                PhoneNumber = model.PhoneNumber,
                Email = model.Email,
                QRCode = qrCode,
                ShopCode = shopCode,
                UserId = userId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Shops.Add(shop);
            await _context.SaveChangesAsync();
            return shop.ShopId;
        }

        public async Task<bool> UpdateShopAsync(ShopViewModel model)
        {
            var shop = await _context.Shops.FindAsync(model.ShopId);
            if (shop == null) return false;

            shop.ShopName = model.ShopName;
            shop.Address = model.Address;
            shop.PhoneNumber = model.PhoneNumber;
            shop.Email = model.Email;
            shop.IsActive = model.IsActive;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteShopAsync(int shopId)
        {
            var shop = await _context.Shops.FindAsync(shopId);
            if (shop == null) return false;

            shop.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RegenerateQRCodeAsync(int shopId, string baseUrl)
        {
            var shop = await _context.Shops.FindAsync(shopId);
            if (shop == null) return false;

            // Generate new QR code with updated URL
            var qrContent = $"{baseUrl}/QRRegistration/Register?shopCode={shop.ShopCode}";
            var qrCode = _qrCodeService.GenerateQRCode(qrContent);

            shop.QRCode = qrCode;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
