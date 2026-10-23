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
        public async Task<IActionResult> Index(
     string? status,
     string? destination,
     string? availability,

     int page = 1)
        {
            int pageSize = 6;

            var query = _context.Trips
                .AsNoTracking()
                .AsQueryable();

            // Filter by status
            if (!string.IsNullOrWhiteSpace(status)
                && Enum.TryParse<TripStatus>(status, true, out var selectedStatus))
            {
                query = query.Where(t => t.Status == selectedStatus);
            }

            // Filter by destination station name or city
            if (!string.IsNullOrWhiteSpace(destination))
            {
                query = query.Where(t =>
                    t.TripStops.Any(s =>
                        s.Station.StationName.Contains(destination) ||
                        s.Station.City.Contains(destination)));
            }
            // Filter by available 

            var now = DateTime.Now;

            if (availability == "available")
            {
                query = query.Where(t =>
                    t.TripDate > now &&
                    t.Status != TripStatus.Cancelled &&
                    t.Status != TripStatus.Completed &&
                    t.Train != null &&
                    t.Train.Capacity >
                        t.Tickets.Count(ticket => ticket.Status != "Cancelled"));
            }
            else if (availability == "unavailable")
            {
                query = query.Where(t =>
                    t.TripDate <= now ||
                    t.Status == TripStatus.Cancelled ||
                    t.Status == TripStatus.Completed ||
                    (t.Train != null &&
                     t.Train.Capacity <=
                        t.Tickets.Count(ticket => ticket.Status != "Cancelled")));
            }

            ViewBag.AvailabilityFilter = availability ?? "";

            // Count filtered results before pagination
            int totalTrips = await query.CountAsync();

            int totalPages = (int)Math.Ceiling(totalTrips / (double)pageSize);

            // Keep the requested page within valid bounds
            page = Math.Max(1, page);
            if (totalPages > 0)
            {
                page = Math.Min(page, totalPages);
            }

            var trips = await query
                .Include(t => t.Train)
                .Include(t => t.TripStops)
                    .ThenInclude(s => s.Station)
                .OrderBy(t => t.TripDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.TotalTrips = totalTrips;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.StatusFilter = status ?? "";
            ViewBag.DestinationFilter = destination ?? "";

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
        public async Task<IActionResult> Create()
        {
            await LoadCreateLists();
            return View(new CreateTripViewModel());
        }
        // POST: /Admin/Trips/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(CreateTripViewModel vm)
        {
            var template = vm.TemplateTripId.HasValue
                ? await _context.Trips
                    .Include(t => t.TripStops)
                    .FirstOrDefaultAsync(t => t.TripId == vm.TemplateTripId.Value)
                : null;

            if (template == null)
            {
                ModelState.AddModelError(
                    nameof(vm.TemplateTripId),
                    "Please select a valid existing route.");
            }

            var stops = (vm.Stops ?? new List<CreateTripStopInput>())
                .Where(s => s.StationId.HasValue)
                .ToList();

            if (stops.Count < 2)
            {
                ModelState.AddModelError(
                    nameof(vm.Stops),
                    "A trip must have at least two stations.");
            }

            var stationIds = stops
                .Select(s => s.StationId!.Value)
                .ToList();

            if (stationIds.Distinct().Count() != stationIds.Count)
            {
                ModelState.AddModelError(
                    nameof(vm.Stops),
                    "A station cannot appear more than once in this trip.");
            }

            var existingStationIds = await _context.Stations
                .Where(s => stationIds.Contains(s.StationId))
                .Select(s => s.StationId)
                .ToListAsync();

            if (existingStationIds.Count != stationIds.Count)
            {
                ModelState.AddModelError(
                    nameof(vm.Stops),
                    "One or more selected stations do not exist.");
            }

            if (template != null && vm.TemplateTripId.HasValue)
            {
                var templateHasStops = await _context.TripStops
                    .AnyAsync(s => s.TripId == template.TripId);

                if (!templateHasStops)
                {
                    ModelState.AddModelError(
                        nameof(vm.TemplateTripId),
                        "The selected route has no stations assigned.");
                }
            }

            if (!ModelState.IsValid)
            {
                await LoadCreateLists(vm.TemplateTripId, vm.TrainId);
                return View(vm);
            }

            var newTrip = new Trip
            {
                RouteName = template!.RouteName,
                TripDate = vm.TripDate!.Value,
                TrainId = vm.TrainId!.Value,
                Price = vm.Price,
                Status = vm.Status,
                TripStops = stops
                    .Select((stop, index) => new TripStop
                    {
                        StationId = stop.StationId!.Value,
                        StopOrder = index + 1,
                        ScheduledTime = stop.ScheduledTime,
                        Platform = stop.Platform ?? string.Empty
                    })
                    .ToList()
            };

            _context.Trips.Add(newTrip);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Trip created successfully with its assigned stations.";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadCreateLists(
            int? selectedTemplateTripId = null,
            int? selectedTrainId = null)
        {
            var tripsWithStops = await _context.Trips
                .AsNoTracking()
                .Where(t => t.RouteName != null && t.RouteName != ""
                            && t.TripStops.Any())
                .OrderBy(t => t.RouteName)
                .ThenBy(t => t.TripId)
                .Select(t => new
                {
                    t.TripId,
                    t.RouteName
                })
                .ToListAsync();

            // Show one template trip per route name.
            var routeOptions = tripsWithStops
                .GroupBy(t => t.RouteName!)
                .Select(g => g.First())
                .Select(t => new SelectListItem
                {
                    Value = t.TripId.ToString(),
                    Text = t.RouteName,
                    Selected = t.TripId == selectedTemplateTripId
                })
                .ToList();

            ViewBag.RouteOptions = routeOptions;

            ViewBag.TrainId = new SelectList(
                await _context.Trains.AsNoTracking().ToListAsync(),
                "TrainId",
                "TrainNumber",
                selectedTrainId);

            ViewBag.Stations = await _context.Stations
                .AsNoTracking()
                .OrderBy(s => s.StationName)
                .Select(s => new SelectListItem
                {
                    Value = s.StationId.ToString(),
                    Text = s.StationName
                })
                .ToListAsync();
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetRouteStops(int id)
        {
            var trip = await _context.Trips
                .AsNoTracking()
                .Include(t => t.TripStops)
                    .ThenInclude(s => s.Station)
                .FirstOrDefaultAsync(t => t.TripId == id);

            if (trip == null)
                return NotFound();

            var stops = trip.TripStops
                .OrderBy(s => s.StopOrder)
                .Select(s => new
                {
                    stationId = s.StationId,
                    stationName = s.Station.StationName,
                    scheduledTime = s.ScheduledTime.HasValue
                        ? s.ScheduledTime.Value.ToString("yyyy-MM-ddTHH:mm")
                        : "",
                    platform = s.Platform ?? ""
                });

            return Json(new
            {
                routeName = trip.RouteName,
                stops
            });
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

        [HttpGet]
        public async Task<IActionResult> Book(int? id)
        {
            if (id == null)
                return NotFound();

            var trip = await _context.Trips
                .AsNoTracking()
                .Include(t => t.Train)
                .Include(t => t.TripStops)
                    .ThenInclude(ts => ts.Station)
                .FirstOrDefaultAsync(t => t.TripId == id);

            if (trip == null)
                return NotFound();

            // Do not allow booking cancelled, completed, or past trips.
            if (trip.Status == TripStatus.Cancelled ||
                trip.Status == TripStatus.Completed ||
                trip.TripDate <= DateTime.Now)
            {
                TempData["Error"] = "This trip is no longer available for booking.";
                return RedirectToAction(nameof(Index));
            }

            var stops = trip.TripStops
                .OrderBy(ts => ts.StopOrder)
                .ToList();

            if (stops.Count < 2)
            {
                TempData["Error"] =
                    "This trip does not have enough stations assigned yet.";

                return RedirectToAction(nameof(Index));
            }

            var bookedSeats = await _context.Tickets
                .Where(t => t.TripId == id && t.Status != "Cancelled")
                .Select(t => t.SeatNumber)
                .ToListAsync();

            int capacity = trip.Train?.Capacity ?? 0;

            var availableSeats = Enumerable.Range(1, Math.Max(0, capacity))
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
                Stops = stops,
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
                return NotFound();

            var trip = await _context.Trips
                .Include(t => t.Train)
                .Include(t => t.TripStops)
                    .ThenInclude(ts => ts.Station)
                .FirstOrDefaultAsync(t => t.TripId == id);

            if (trip == null)
                return NotFound();

            // Check whether the trip can still be booked.
            if (trip.Status == TripStatus.Cancelled ||
                trip.Status == TripStatus.Completed ||
                trip.TripDate <= DateTime.Now)
            {
                TempData["Error"] =
                    "This trip is no longer available for booking.";

                return RedirectToAction(nameof(Index));
            }

            // Validate the ticket class.
            var validClasses = new[] { "Economy", "Business", "VIP" };

            if (string.IsNullOrWhiteSpace(vm.TravelClass) ||
                !validClasses.Contains(vm.TravelClass))
            {
                ModelState.AddModelError(
                    nameof(vm.TravelClass),
                    "Please choose a valid ticket class.");
            }

            // Validate boarding and destination stations.
            var fromStop = trip.TripStops
                .FirstOrDefault(s => s.StationId == vm.FromStationId);

            var toStop = trip.TripStops
                .FirstOrDefault(s => s.StationId == vm.ToStationId);

            if (fromStop == null || toStop == null)
            {
                ModelState.AddModelError(
                    "",
                    "Choose stations that belong to this trip.");
            }
            else if (fromStop.StopOrder >= toStop.StopOrder)
            {
                ModelState.AddModelError(
                    "",
                    "The boarding station must come before the destination station.");
            }

            // Find seats already booked on this trip.
            var bookedSeats = await _context.Tickets
                .Where(t => t.TripId == id && t.Status != "Cancelled")
                .Select(t => t.SeatNumber)
                .ToListAsync();

            int capacity = trip.Train?.Capacity ?? 0;

            var availableSeats = Enumerable
                .Range(1, Math.Max(0, capacity))
                .Select(n => n.ToString())
                .Except(bookedSeats)
                .ToList();

            // Clean up the selected seats and remove duplicates.
            vm.SeatNumbers = (vm.SeatNumbers ?? new List<string>())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct()
                .ToList();

            if (vm.SeatNumbers.Count > 6)
            {
                ModelState.AddModelError(
                    nameof(vm.SeatNumbers),
                    "You can book a maximum of 6 seats at a time.");
            }

            // Validate all selected seats.
            if (vm.SeatNumbers.Count == 0)
            {
                ModelState.AddModelError(
                    nameof(vm.SeatNumbers),
                    "Please choose at least one seat.");
            }
            else if (vm.SeatNumbers.Any(s => !availableSeats.Contains(s)))
            {
                ModelState.AddModelError(
                    nameof(vm.SeatNumbers),
                    "One or more selected seats are unavailable. Please choose again.");
            }

            // If validation fails, return to the booking page.
            if (!ModelState.IsValid)
            {
                vm.RouteName = trip.RouteName;
                vm.TripDate = trip.TripDate;
                vm.TrainNumber = trip.Train?.TrainNumber;
                vm.Price = trip.Price;

                vm.Stops = trip.TripStops
                    .OrderBy(s => s.StopOrder)
                    .ToList();

                vm.AvailableSeats = availableSeats;

                return View("BookTrip", vm);
            }

            // Get the logged-in user's ID.
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Challenge();

            // Calculate the price for each ticket.
            decimal multiplier = vm.TravelClass switch
            {
                "Business" => 1.5m,
                "VIP" => 2.0m,
                _ => 1.0m
            };

            decimal ticketPrice = trip.Price * multiplier;


            // Create tickets and keep their IDs for the payment page.
            var tickets = new List<Ticket>();

            foreach (var seat in vm.SeatNumbers)
            {
                var ticket = new Ticket
                {
                    TripId = trip.TripId,
                    FromStationId = vm.FromStationId,
                    ToStationId = vm.ToStationId,
                    SeatNumber = seat,
                    TravelClass = vm.TravelClass,
                    Status = "PendingPayment",
                    BookingDate = DateTime.Now,
                    Price = ticketPrice,
                    UserId = userId
                };

                tickets.Add(ticket);
            }

            _context.Tickets.AddRange(tickets);

            await _context.SaveChangesAsync();

            // The database has now generated the TicketIds.
            var ticketIds = tickets
                .Select(t => t.TicketId)
                .ToArray();

            // Redirect to the customer payment page.
            return RedirectToAction(
                "ChooseMethod",
                "Payments",
                new
                {
                    area = "Customer",
                    tripId = trip.TripId,
                    ticketIds = ticketIds
                });

        }


    }
}