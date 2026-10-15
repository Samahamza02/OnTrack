using Microsoft.AspNetCore.Mvc;
using OnTrack.Interfaces;

namespace OnTrack.Controllers
{
    public class NotificationsController : Controller
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        // GET: /Notifications
        public async Task<IActionResult> Index()
        {
            var notifications = await _notificationService.GetAllAsync();

            return View(notifications);
        }

        // GET: /Notifications/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var notification = await _notificationService.GetByIdAsync(id);

            if (notification == null)
            {
                return NotFound();
            }

            return View(notification);
        }
    }
}