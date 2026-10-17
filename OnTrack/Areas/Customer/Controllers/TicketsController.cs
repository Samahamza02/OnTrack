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

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var tickets = await _ticketService.GetAllAsync();

            var myTickets = tickets
                .Where(t => t.UserId == userId);

            return View(myTickets);
        }

        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);

            var ticket = await _ticketService.GetByIdAsync(id);

            if (ticket == null || ticket.UserId != userId)
                return NotFound();

            return View(ticket);
        }
    }
}