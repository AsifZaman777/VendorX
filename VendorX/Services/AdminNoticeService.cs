using Microsoft.EntityFrameworkCore;
using VendorX.Models;
using VendorX.ViewModels;

namespace VendorX.Services
{
    public interface IAdminNoticeService
    {
        Task<List<AdminNoticeViewModel>> GetAllNoticesAsync();
        Task<List<AdminNoticeViewModel>> GetActiveNoticesForRoleAsync(string? role);
        Task<AdminNoticeViewModel?> GetNoticeByIdAsync(int noticeId);
        Task<int> CreateNoticeAsync(AdminNoticeViewModel model);
        Task<bool> UpdateNoticeAsync(AdminNoticeViewModel model);
        Task<bool> DeleteNoticeAsync(int noticeId);
        Task<bool> ToggleNoticeStatusAsync(int noticeId);
    }

    public class AdminNoticeService : IAdminNoticeService
    {
        private readonly ApplicationDbContext _context;

        public AdminNoticeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<AdminNoticeViewModel>> GetAllNoticesAsync()
        {
            return await _context.AdminNotices
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new AdminNoticeViewModel
                {
                    NoticeId = n.NoticeId,
                    Title = n.Title,
                    Message = n.Message,
                    NoticeType = n.NoticeType,
                    IsActive = n.IsActive,
                    CreatedAt = n.CreatedAt,
                    ExpiresAt = n.ExpiresAt,
                    TargetRole = n.TargetRole
                })
                .ToListAsync();
        }

        public async Task<List<AdminNoticeViewModel>> GetActiveNoticesForRoleAsync(string? role)
        {
            var now = DateTime.UtcNow;
            
            return await _context.AdminNotices
                .Where(n => n.IsActive && 
                           (n.ExpiresAt == null || n.ExpiresAt > now) &&
                           (n.TargetRole == null || n.TargetRole == role))
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new AdminNoticeViewModel
                {
                    NoticeId = n.NoticeId,
                    Title = n.Title,
                    Message = n.Message,
                    NoticeType = n.NoticeType,
                    IsActive = n.IsActive,
                    CreatedAt = n.CreatedAt,
                    ExpiresAt = n.ExpiresAt,
                    TargetRole = n.TargetRole
                })
                .ToListAsync();
        }

        public async Task<AdminNoticeViewModel?> GetNoticeByIdAsync(int noticeId)
        {
            var notice = await _context.AdminNotices.FindAsync(noticeId);
            if (notice == null) return null;

            return new AdminNoticeViewModel
            {
                NoticeId = notice.NoticeId,
                Title = notice.Title,
                Message = notice.Message,
                NoticeType = notice.NoticeType,
                IsActive = notice.IsActive,
                CreatedAt = notice.CreatedAt,
                ExpiresAt = notice.ExpiresAt,
                TargetRole = notice.TargetRole
            };
        }

        public async Task<int> CreateNoticeAsync(AdminNoticeViewModel model)
        {
            var notice = new AdminNotice
            {
                Title = model.Title,
                Message = model.Message,
                NoticeType = model.NoticeType,
                IsActive = model.IsActive,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = model.ExpiresAt,
                TargetRole = model.TargetRole
            };

            _context.AdminNotices.Add(notice);
            await _context.SaveChangesAsync();
            return notice.NoticeId;
        }

        public async Task<bool> UpdateNoticeAsync(AdminNoticeViewModel model)
        {
            var notice = await _context.AdminNotices.FindAsync(model.NoticeId);
            if (notice == null) return false;

            notice.Title = model.Title;
            notice.Message = model.Message;
            notice.NoticeType = model.NoticeType;
            notice.IsActive = model.IsActive;
            notice.ExpiresAt = model.ExpiresAt;
            notice.TargetRole = model.TargetRole;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteNoticeAsync(int noticeId)
        {
            var notice = await _context.AdminNotices.FindAsync(noticeId);
            if (notice == null) return false;

            _context.AdminNotices.Remove(notice);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleNoticeStatusAsync(int noticeId)
        {
            var notice = await _context.AdminNotices.FindAsync(noticeId);
            if (notice == null) return false;

            notice.IsActive = !notice.IsActive;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
