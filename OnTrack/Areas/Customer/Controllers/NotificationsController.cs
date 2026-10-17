using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnTrack.Interfaces;
using OnTrack.Models;

namespace OnTrack.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize(Roles = "Customer")]
    public class NotificationsController : Controller
    {
        private readonly INotificationService _notificationService;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationsController(
            INotificationService notificationService,
            UserManager<ApplicationUser> userManager)
        {
            _notificationService = notificationService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var notifications =
                await _notificationService.GetAllAsync();

            var myNotifications = notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.SentAt);

            return View(myNotifications);
        }

        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);

            var notification =
                await _notificationService.GetByIdAsync(id);

            if (notification == null ||
                notification.UserId != userId)
            {
                return NotFound();
            }

            return View(notification);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = _userManager.GetUserId(User);

            var notification =
                await _notificationService.GetByIdAsync(id);

            if (notification == null || notification.UserId != userId)
            {
                return NotFound();
            }

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                await _notificationService.UpdateAsync(notification);
            }

            return RedirectToAction(nameof(Index));
        }

    }
}