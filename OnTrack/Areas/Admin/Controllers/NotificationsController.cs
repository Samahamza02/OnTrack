using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnTrack.Data;
using OnTrack.Interfaces;
using OnTrack.Models;

namespace OnTrack.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class NotificationsController : Controller
    {
        private readonly INotificationService _notificationService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public NotificationsController(
            INotificationService notificationService,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _notificationService = notificationService;
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var notifications =
                await _notificationService.GetAllAsync();

            return View(notifications.OrderByDescending(n => n.SentAt));
        }

        public async Task<IActionResult> Details(int id)
        {
            var notification =
                await _notificationService.GetByIdAsync(id);

            if (notification == null)
                return NotFound();

            return View(notification);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadDropdownsAsync();
            return View();
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var notification = await _notificationService.GetByIdAsync(id);

            if (notification == null)
                return NotFound();

            await LoadDropdownsAsync();

            return View(notification);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id, Notification notification)
        {
            if (id != notification.NotificationId)
                return BadRequest();

            if (string.IsNullOrWhiteSpace(notification.Message))
            {
                ModelState.AddModelError(
                    nameof(notification.Message),
                    "Please enter the notification message.");
            }

            var userExists = await _userManager.Users
                .AnyAsync(u => u.Id == notification.UserId);

            if (!userExists)
            {
                ModelState.AddModelError(
                    nameof(notification.UserId),
                    "Please select a valid user.");
            }

            if (notification.TripId.HasValue)
            {
                var tripExists = await _context.Trips
                    .AnyAsync(t => t.TripId == notification.TripId.Value);

                if (!tripExists)
                {
                    ModelState.AddModelError(
                        nameof(notification.TripId),
                        "Please select a valid trip.");
                }
            }

            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync();
                return View(notification);
            }

            var existing = await _notificationService.GetByIdAsync(id);

            if (existing == null)
                return NotFound();

            // Preserve fields managed by the application.
            existing.Message = notification.Message;
            existing.UserId = notification.UserId;
            existing.TripId = notification.TripId;

            var updated = await _notificationService.UpdateAsync(existing);

            if (!updated)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "The notification could not be updated.");

                await LoadDropdownsAsync();
                return View(notification);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var notification = await _notificationService.GetByIdAsync(id);

            if (notification == null)
                return NotFound();

            return View(notification);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var deleted = await _notificationService.DeleteAsync(id);

            if (!deleted)
                return NotFound();

            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Notification notification)
        {
            if (string.IsNullOrWhiteSpace(notification.Message))
            {
                ModelState.AddModelError(
                    nameof(notification.Message),
                    "Please enter the notification message.");
            }

            var userExists = await _userManager.Users
                .AnyAsync(u => u.Id == notification.UserId);

            if (!userExists)
            {
                ModelState.AddModelError(
                    nameof(notification.UserId),
                    "Please select a valid user.");
            }

            if (notification.TripId.HasValue)
            {
                var tripExists = await _context.Trips
                    .AnyAsync(t => t.TripId == notification.TripId.Value);

                if (!tripExists)
                {
                    ModelState.AddModelError(
                        nameof(notification.TripId),
                        "Please select a valid trip.");
                }
            }

            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync();
                return View(notification);
            }

            notification.SentAt = DateTime.Now;
            notification.IsRead = false;

            var created =
                await _notificationService.CreateAsync(notification);

            if (!created)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "The notification could not be created.");

                await LoadDropdownsAsync();
                return View(notification);
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task LoadDropdownsAsync()
        {
            var users = await _userManager.Users.ToListAsync();

            ViewBag.Users = new SelectList(
                users,
                "Id",
                "UserName");

            var trips = await _context.Trips.ToListAsync();

            ViewBag.Trips = new SelectList(
                trips,
                "TripId",
                "TripId");
        }
    }
}