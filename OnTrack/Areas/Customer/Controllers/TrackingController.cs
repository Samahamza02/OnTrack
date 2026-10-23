
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnTrack.Data;
using OnTrack.Models;
using OnTrack.Services;

namespace OnTrack.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class TrackingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly TripSimulationService _simulationService;

        public TrackingController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            TripSimulationService simulationService)
        {
            _context = context;
            _userManager = userManager;
            _simulationService = simulationService;
        }

        // Customer opens the tracking page for a booked trip.
        public async Task<IActionResult> Track(int id)
        {
            var userId = _userManager.GetUserId(User);

            if (userId == null)
                return Challenge();

            // Check that this customer owns a ticket for this trip.
            var ticketExists = await _context.Tickets
                .AnyAsync(t =>
                    t.TripId == id &&
                    t.UserId == userId);

            if (!ticketExists)
                return Forbid();

            var trip = await _context.Trips
                .Include(t => t.Train)
                .Include(t => t.TripStops)
                    .ThenInclude(s => s.Station)
                .FirstOrDefaultAsync(t => t.TripId == id);

            if (trip == null)
                return NotFound();

            if (trip.Status == TripStatus.Cancelled)
            {
                TempData["TrackingMessage"] =
                    "This trip has been cancelled.";

                return RedirectToAction("Index", "Tickets",
                    new { area = "Customer" });
            }

            // Start only if the simulation is not already running.
            if (trip.Status != TripStatus.Completed)
                _simulationService.StartSimulation(id);

            return View(trip);
        }
    }
}
