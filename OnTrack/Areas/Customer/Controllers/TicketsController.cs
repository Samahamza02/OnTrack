
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnTrack.Interfaces;
using OnTrack.Models;

namespace OnTrack.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize(Roles = "Customer")]
    public class TicketsController : Controller
    {
        private readonly ITicketService _ticketService;
        private readonly UserManager<ApplicationUser> _userManager;

        public TicketsController(
            ITicketService ticketService,
            UserManager<ApplicationUser> userManager)
        {
            _ticketService = ticketService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(
            string? status,
            string? search)
        {
            var userId = _userManager.GetUserId(User);

            if (userId == null)
                return Challenge();

            var allTickets = await _ticketService.GetAllAsync();

            var myTickets = allTickets
                .Where(t => t.UserId == userId)
                .ToList();

            ViewBag.TotalTickets = myTickets.Count;

            ViewBag.PendingPayment = myTickets.Count(t =>
                string.Equals(t.Status, "Pending",
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(t.Status, "Pending Payment",
                    StringComparison.OrdinalIgnoreCase));

            ViewBag.ConfirmedTickets = myTickets.Count(t =>
                string.Equals(t.Status, "Confirmed",
                    StringComparison.OrdinalIgnoreCase));

            ViewBag.SelectedStatus = status ?? "";
            ViewBag.Search = search ?? "";

            var filteredTickets = myTickets.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status == "Pending")
                {
                    filteredTickets = filteredTickets.Where(t =>
                        string.Equals(t.Status, "Pending",
                            StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(t.Status, "Pending Payment",
                            StringComparison.OrdinalIgnoreCase));
                }
                else
                {
                    filteredTickets = filteredTickets.Where(t =>
                        string.Equals(t.Status, status,
                            StringComparison.OrdinalIgnoreCase));
                }
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                filteredTickets = filteredTickets.Where(t =>
                    t.TicketId.ToString().Contains(search,
                        StringComparison.OrdinalIgnoreCase) ||
                    t.TripId.ToString().Contains(search,
                        StringComparison.OrdinalIgnoreCase) ||
                    t.SeatNumber.Contains(search,
                        StringComparison.OrdinalIgnoreCase) ||
                    t.FromStationId.ToString().Contains(search,
                        StringComparison.OrdinalIgnoreCase) ||
                    t.ToStationId.ToString().Contains(search,
                        StringComparison.OrdinalIgnoreCase));
            }

            return View(filteredTickets
                .OrderByDescending(t => t.BookingDate)
                .ToList());
        }

        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);

            var tickets = await _ticketService.GetAllAsync();

            var ticket = tickets.FirstOrDefault(t =>
                t.TicketId == id && t.UserId == userId);

            if (ticket == null)
                return NotFound();

            return View(ticket);
        }
    }
}
