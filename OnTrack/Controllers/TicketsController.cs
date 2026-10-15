using Microsoft.AspNetCore.Mvc;
using OnTrack.Interfaces;

namespace OnTrack.Controllers
{
    public class TicketsController : Controller
    {
        private readonly ITicketService _ticketService;

        public TicketsController(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        // GET: /Tickets
        public async Task<IActionResult> Index()
        {
            var tickets = await _ticketService.GetAllAsync();

            return View(tickets);
        }

        // GET: /Tickets/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var ticket = await _ticketService.GetByIdAsync(id);

            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }
    }
}
