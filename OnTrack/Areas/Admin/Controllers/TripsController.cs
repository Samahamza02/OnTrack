using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnTrack.Data;
using OnTrack.Models;
using OnTrack.Models.ViewModels;
using System.Security.Claims;

namespace OnTrack.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class TripsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TripsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Trip
        public async Task<IActionResult> Index()
        {
            var trips = await _context.Trips
                .AsNoTracking()
                .Include(t => t.Train)
                .ToListAsync();

            return View(trips);
        }

        // GET: Admin/Trip/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trip = await _context.Trips
                .AsNoTracking()
                .Include(t => t.Train)
                .FirstOrDefaultAsync(t => t.TripId == id);

            if (trip == null)
            {
                return NotFound();
            }

            return View(trip);
        }

        //// GET: Admin/Trip/Create
        //[Authorize(Roles = "Admin")]
        //public IActionResult Create()
        //{
        //    ViewBag.TrainId = new SelectList(_context.Trains, "TrainId", "TrainNumber");
        //    return View();
        //}

        //// POST: Admin/Trip/Create
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[Authorize(Roles = "Admin")]
        //public async Task<IActionResult> Create(Trip trip)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _context.Trips.Add(trip);
        //        await _context.SaveChangesAsync();

        //        return RedirectToAction(nameof(Index));
        //    }

        //    return View(trip);
        //}

        //// GET: Admin/Trip/Edit/5
        //[Authorize(Roles = "Admin")]
        //public async Task<IActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var trip = await _context.Trips.FindAsync(id);

        //    if (trip == null)
        //    {
        //        return NotFound();
        //    }
        //    ViewBag.TrainId = new SelectList(_context.Trains, "TrainId", "TrainNumber");
        //    return View(trip);
        //}

        //// POST: Admin/Trip/Edit/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[Authorize(Roles = "Admin")]
        //public async Task<IActionResult> Edit(int id, Trip trip)
        //{
        //    if (id != trip.TripId)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _context.Update(trip);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!TripExists(trip.TripId))
        //            {
        //                return NotFound();
        //            }

        //            throw;
        //        }

        //        return RedirectToAction(nameof(Index));
        //    }

        //    return View(trip);
        //}

        //// GET: Admin/Trip/Delete/5
        //[Authorize(Roles = "Admin")]
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var trip = await _context.Trips
        //        .AsNoTracking()
        //        .Include(t => t.Train)
        //        .FirstOrDefaultAsync(t => t.TripId == id);

        //    if (trip == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(trip);
        //}

        //// POST: Admin/Trip/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //[Authorize(Roles = "Admin")]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    var trip = await _context.Trips.FindAsync(id);

        //    if (trip != null)
        //    {
        //        _context.Trips.Remove(trip);
        //        await _context.SaveChangesAsync();
        //    }

        //    return RedirectToAction(nameof(Index));
        //}

        //private bool TripExists(int id)
        //{
        //    return _context.Trips.Any(t => t.TripId == id);
        //}


        // GET: /Admin/Trips/Create
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            LoadTrains();
            return View();
        }

        // POST: /Admin/Trips/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Trip trip)
        {
            if (!ModelState.IsValid)
            {
                LoadTrains(trip.TrainId);
                return View(trip);
            }

            _context.Trips.Add(trip);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Trip created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/Trips/Edit/5
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var trip = await _context.Trips.FindAsync(id);

            if (trip == null)
                return NotFound();

            LoadTrains(trip.TrainId);
            return View(trip);
        }

        // POST: /Admin/Trips/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Trip trip)
        {
            if (id != trip.TripId)
                return NotFound();

            if (!ModelState.IsValid)
            {
                LoadTrains(trip.TrainId);
                return View(trip);
            }

            var existingTrip = await _context.Trips.FindAsync(id);

            if (existingTrip == null)
                return NotFound();

            // Update the existing database record.
            existingTrip.RouteName = trip.RouteName;
            existingTrip.TripDate = trip.TripDate;
            existingTrip.Status = trip.Status;
            existingTrip.TrainId = trip.TrainId;
            existingTrip.Price = trip.Price;

            try
            {
                await _context.SaveChangesAsync();
                TempData["Success"] = "Trip updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Trips.AnyAsync(t => t.TripId == id))
                    return NotFound();

                throw;
            }
        }

        // GET: /Admin/Trips/Delete/5
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var trip = await _context.Trips
                .AsNoTracking()
                .Include(t => t.Train)
                .FirstOrDefaultAsync(t => t.TripId == id);

            if (trip == null)
                return NotFound();

            return View(trip);
        }

        // POST: /Admin/Trips/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var trip = await _context.Trips.FindAsync(id);

            if (trip == null)
                return NotFound();

            // Do not delete a trip that still has tickets.
            bool hasTickets = await _context.Tickets
                .AnyAsync(t => t.TripId == id);

            if (hasTickets)
            {
                TempData["Error"] =
                    "This trip cannot be deleted because tickets are linked to it.";

                return RedirectToAction(nameof(Index));
            }

            // Remove related route stops first, if any.
            var stops = await _context.TripStops
                .Where(s => s.TripId == id)
                .ToListAsync();

            _context.TripStops.RemoveRange(stops);
            _context.Trips.Remove(trip);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Trip deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        private void LoadTrains(int? selectedTrainId = null)
        {
            ViewBag.TrainId = new SelectList(
                _context.Trains.AsNoTracking().ToList(),
                "TrainId",
                "TrainNumber",
                selectedTrainId);
        }




        // GET: Admin/Trip/Book/5
        public async Task<IActionResult> Book(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trip = await _context.Trips
                .AsNoTracking()
                .Include(t => t.Train)
                .Include(t => t.TripStops)
                    .ThenInclude(ts => ts.Station)
                .FirstOrDefaultAsync(t => t.TripId == id);

            if (trip == null)
            {
                return NotFound();
            }
            var bookedSeats = await _context.Tickets
                .Where(t => t.TripId == id && t.Status != "Cancelled")
                .Select(t => t.SeatNumber)
                .ToListAsync();

            var availableSeats = Enumerable.Range(1, trip.Train.Capacity)
                .Select(n => n.ToString())
                .Except(bookedSeats)
                .ToList();

            var vm = new BookTripViewModel
            {
                TripId = trip.TripId,
                RouteName = trip.RouteName,
                TripDate = trip.TripDate,
                TrainNumber = trip.Train?.TrainNumber,
                Price = trip.Price,
                Stops = trip.TripStops.OrderBy(ts => ts.StopOrder).ToList(),
                AvailableSeats = availableSeats
            };

            return View(vm);
        }

        // POST: Admin/Trip/Book/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(int id, BookTripViewModel vm)
        {
            if (id != vm.TripId)
            {
                return NotFound();
            }

            var trip = await _context.Trips
                .Include(t => t.Train)
                .Include(t => t.TripStops)
                    .ThenInclude(ts => ts.Station)
                .FirstOrDefaultAsync(t => t.TripId == id);

            if (trip == null)
            {
                return NotFound();
            }

            var fromStop = trip.TripStops.FirstOrDefault(ts => ts.StationId == vm.FromStationId);
            var toStop = trip.TripStops.FirstOrDefault(ts => ts.StationId == vm.ToStationId);

            if (fromStop == null || toStop == null)
            {
                ModelState.AddModelError("", "The selected station isn’t part of this trip route.");
            }
            else if (fromStop.StopOrder >= toStop.StopOrder)
            {
                ModelState.AddModelError("", "The boarding station must be before the destination station in the trip route.");
            }

            var seatTaken = await _context.Tickets
                .AnyAsync(t => t.TripId == id
                            && t.SeatNumber == vm.SeatNumber
                            && t.Status != "Cancelled");

            if (seatTaken)
            {
                ModelState.AddModelError("", "The seat is already booked for this trip.");
            }

            if (!ModelState.IsValid)
            {
                vm.RouteName = trip.RouteName;
                vm.TripDate = trip.TripDate;
                vm.TrainNumber = trip.Train?.TrainNumber;
                vm.Price = trip.Price;
                vm.Stops = trip.TripStops.OrderBy(ts => ts.StopOrder).ToList();

                var bookedSeats = await _context.Tickets
                    .Where(t => t.TripId == id && t.Status != "Cancelled")
                    .Select(t => t.SeatNumber)
                    .ToListAsync();

                vm.AvailableSeats = Enumerable.Range(1, trip.Train.Capacity)
                    .Select(n => n.ToString())
                    .Except(bookedSeats)
                    .ToList();

                return View(vm);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var ticket = new Ticket
            {
                TripId = vm.TripId,
                FromStationId = vm.FromStationId,
                ToStationId = vm.ToStationId,
                SeatNumber = vm.SeatNumber,
                Status = "Booked",
                BookingDate = DateTime.Now,
                Price = trip.Price,
                UserId = userId!
            };

            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}