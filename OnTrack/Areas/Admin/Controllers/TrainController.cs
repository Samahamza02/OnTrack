using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnTrack.Data;
using OnTrack.Models;

namespace OnTrack.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class TrainController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TrainController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Trains
        public async Task<IActionResult> Index()
        {
            var trains = await _context.Trains
                .AsNoTracking()
                .ToListAsync();

            return View(trains);
        }
        [Authorize(Roles = "Admin")]
        // GET: Admin/Trains/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var train = await _context.Trains
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TrainId == id);

            if (train == null)
            {
                return NotFound();
            }

            return View(train);
        }

        // GET: Admin/Trains/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Trains/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Train train)
        {
            if (ModelState.IsValid)
            {
                _context.Trains.Add(train);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(train);
        }

        // GET: Admin/Trains/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var train = await _context.Trains.FindAsync(id);

            if (train == null)
            {
                return NotFound();
            }

            return View(train);
        }

        // POST: Admin/Trains/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Train train)
        {
            if (id != train.TrainId)
            {
                return BadRequest($"ID mismatch: URL={id}, Model={train.TrainId}");
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value != null && x.Value.Errors.Count > 0)
                    .SelectMany(x => x.Value!.Errors.Select(e =>
                        $"{x.Key}: {e.ErrorMessage}"));

                ViewBag.ValidationErrors = string.Join(" | ", errors);

                return View(train);
            }

            var existingTrain = await _context.Trains.FindAsync(id);

            if (existingTrain == null)
            {
                return NotFound();
            }

            existingTrain.TrainNumber = train.TrainNumber;
            existingTrain.TrainName = train.TrainName;
            existingTrain.Capacity = train.Capacity;
            existingTrain.TrainType = train.TrainType;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Trains/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var train = await _context.Trains
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TrainId == id);

            if (train == null)
            {
                return NotFound();
            }

            return View(train);
        }

        // POST: Admin/Trains/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var train = await _context.Trains.FindAsync(id);

            if (train != null)
            {
                _context.Trains.Remove(train);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool TrainExists(int id)
        {
            return _context.Trains.Any(t => t.TrainId == id);
        }
    }
}