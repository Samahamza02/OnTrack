
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnTrack.Interfaces;
using OnTrack.Models;

namespace OnTrack.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class TicketsController : Controller
    {
        private readonly ITicketService _ticketService;
        private readonly INotificationService _notificationService;
        private readonly UserManager<ApplicationUser> _userManager;

        public TicketsController(
            ITicketService ticketService,
            INotificationService notificationService,
            UserManager<ApplicationUser> userManager)
        {
            _ticketService = ticketService;
            _notificationService = notificationService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(
            string? status,
            string? search)
        {
            var tickets = (await _ticketService.GetAllAsync()).ToList();

            var users = await _userManager.Users
                .ToDictionaryAsync(u => u.Id, u => u.UserName ?? "Unknown");

            ViewBag.PassengerNames = users;
            ViewBag.TotalTickets = tickets.Count;

            ViewBag.Pending = tickets.Count(t =>
                IsStatus(t.Status, "Pending") ||
                IsStatus(t.Status, "Pending Payment"));

            ViewBag.Confirmed = tickets.Count(t =>
                IsStatus(t.Status, "Confirmed"));

            ViewBag.Revenue = tickets
                .Where(t => !IsStatus(t.Status, "Cancelled")
                         && !IsStatus(t.Status, "Canceled"))
                .Sum(t => t.Price);

            ViewBag.SelectedStatus = status ?? "";
            ViewBag.Search = search ?? "";

            IEnumerable<Ticket> result = tickets;

            if (!string.IsNullOrWhiteSpace(status))
            {
                result = result.Where(t =>
                    string.Equals(t.Status, status,
                        StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                result = result.Where(t =>
                    t.TicketId.ToString().Contains(search,
                        StringComparison.OrdinalIgnoreCase) ||
                    t.TripId.ToString().Contains(search,
                        StringComparison.OrdinalIgnoreCase) ||
                    t.SeatNumber.Contains(search,
                        StringComparison.OrdinalIgnoreCase) ||
                    t.FromStationId.ToString().Contains(search,
                        StringComparison.OrdinalIgnoreCase) ||
                    t.ToStationId.ToString().Contains(search,
                        StringComparison.OrdinalIgnoreCase) ||
                    (users.TryGetValue(t.UserId, out var name) &&
                     name.Contains(search,
                        StringComparison.OrdinalIgnoreCase)));
            }

            return View(result.OrderByDescending(t => t.BookingDate).ToList());
        }

        public async Task<IActionResult> Details(int id)
        {
            var ticket = await _ticketService.GetByIdAsync(id);

            if (ticket == null)
                return NotFound();

            return View(ticket);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(int id)
        {
            var ticket = await _ticketService.GetByIdAsync(id);

            if (ticket == null)
                return NotFound();

            if (!IsStatus(ticket.Status, "Pending") &&
                !IsStatus(ticket.Status, "Pending Payment"))
            {
                TempData["Error"] = "Only pending tickets can be confirmed.";
                return RedirectToAction(nameof(Index));
            }

            ticket.Status = "Confirmed";

            var updated = await _ticketService.UpdateAsync(ticket);

            TempData[updated ? "Success" : "Error"] = updated
                ? "Ticket confirmed successfully."
                : "Could not update the ticket.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var ticket = await _ticketService.GetByIdAsync(id);

            if (ticket == null)
                return NotFound();

            if (!IsStatus(ticket.Status, "Pending") &&
                !IsStatus(ticket.Status, "Pending Payment") &&
                !IsStatus(ticket.Status, "Confirmed"))
            {
                TempData["Error"] = "This ticket cannot be cancelled.";
                return RedirectToAction(nameof(Index));
            }

            ticket.Status = "Cancelled";

            var updated = await _ticketService.UpdateAsync(ticket);

            TempData[updated ? "Success" : "Error"] = updated
                ? "Ticket cancelled successfully."
                : "Could not cancel the ticket.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id)
        {
            var ticket = await _ticketService.GetByIdAsync(id);

            if (ticket == null)
                return NotFound();

            if (!IsStatus(ticket.Status, "Confirmed"))
            {
                TempData["Error"] =
                    "Only confirmed tickets can be marked as completed.";

                return RedirectToAction(nameof(Index));
            }

            ticket.Status = "Completed";

            var updated = await _ticketService.UpdateAsync(ticket);

            TempData[updated ? "Success" : "Error"] = updated
                ? "Ticket marked as completed."
                : "Could not update the ticket.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Notify(int id)
        {
            var ticket = await _ticketService.GetByIdAsync(id);

            if (ticket == null)
                return NotFound();

            var notification = new Notification
            {
                UserId = ticket.UserId,
                TripId = ticket.TripId,
                Message = $"Update regarding your ticket #{ticket.TicketId}. Current status: {ticket.Status}.",
                SentAt = DateTime.Now,
                IsRead = false
            };

            var created =
                await _notificationService.CreateAsync(notification);

            TempData[created ? "Success" : "Error"] = created
                ? "Notification created successfully."
                : "Could not create the notification.";

            return RedirectToAction(nameof(Index));
        }

        private static bool IsStatus(string? actual, string expected)
        {
            return string.Equals(
                actual?.Trim(),
                expected,
                StringComparison.OrdinalIgnoreCase);
        }
    }
}
