using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendorX.Services;
using VendorX.ViewModels;

namespace VendorX.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin")]
    [Authorize(Roles = "SuperAdmin")]
    public class AdminNoticesController : Controller
    {
        private readonly IAdminNoticeService _noticeService;

        public AdminNoticesController(IAdminNoticeService noticeService)
        {
            _noticeService = noticeService;
        }

        // GET: SuperAdmin/AdminNotices
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var notices = await _noticeService.GetAllNoticesAsync();
            return View(notices);
        }

        // POST: SuperAdmin/AdminNotices/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminNoticeViewModel model)
        {
            // Remove validation for fields that are auto-generated
            ModelState.Remove("NoticeId");
            ModelState.Remove("CreatedAt");
            
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                TempData["Error"] = $"Failed to create notice: {string.Join(", ", errors)}";
                return RedirectToAction(nameof(Index));
            }

            await _noticeService.CreateNoticeAsync(model);
            TempData["Success"] = "Notice created successfully!";
            return RedirectToAction(nameof(Index));
        }

        // POST: SuperAdmin/AdminNotices/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AdminNoticeViewModel model)
        {
            // Remove validation for auto-generated field
            ModelState.Remove("CreatedAt");
            
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                TempData["Error"] = $"Failed to update notice: {string.Join(", ", errors)}";
                return RedirectToAction(nameof(Index));
            }

            var result = await _noticeService.UpdateNoticeAsync(model);
            if (result)
            {
                TempData["Success"] = "Notice updated successfully!";
            }
            else
            {
                TempData["Error"] = "Notice not found.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: SuperAdmin/AdminNotices/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _noticeService.DeleteNoticeAsync(id);
            if (result)
            {
                TempData["Success"] = "Notice deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Notice not found.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: SuperAdmin/AdminNotices/ToggleStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var result = await _noticeService.ToggleNoticeStatusAsync(id);
            if (result)
            {
                TempData["Success"] = "Notice status updated!";
            }
            else
            {
                TempData["Error"] = "Notice not found.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
