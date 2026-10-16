
using Microsoft.AspNetCore.Mvc;
using OnTrack.Interfaces;
using OnTrack.Models;

namespace OnTrack.Controllers
{
    public class TripStopsController : Controller
    {
        private readonly ITripStopService _tripStopService;

        public TripStopsController(
            ITripStopService tripStopService)
        {
            _tripStopService = tripStopService;
        }

        // GET: /TripStops
        public async Task<IActionResult> Index()
        {
            var tripStops = await _tripStopService.GetAllAsync();

            return View(tripStops);
        }

        // GET: /TripStops/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var tripStop = await _tripStopService.GetByIdAsync(id);

            if (tripStop == null)
            {
                return NotFound();
            }

            return View(tripStop);
        }
    }
}
