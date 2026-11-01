
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnTrack.Data;
using OnTrack.Models;

namespace OnTrack.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class EvaluationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EvaluationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/Evaluations
        public async Task<IActionResult> Index(
            int? rating,
            string? search)
        {
            // Get all evaluations for all customers
            var query = _context.Evaluations
                .Include(e => e.Ticket)
                .AsQueryable();

            // Filter by rating
            if (rating.HasValue &&
                rating.Value >= 1 &&
                rating.Value <= 5)
            {
                query = query.Where(
                    e => e.rating == rating.Value);
            }

            // Search by comment or ticket ID
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(e =>
                    (e.comment != null &&
                     e.comment.Contains(search)) ||
                    e.TicketId.ToString().Contains(search));
            }

            // Get filtered evaluations
            var evaluations = await query
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();

            // Dashboard statistics
            var model = new EvaluationDashboardViewModel
            {
                Evaluations = evaluations,

                SelectedRating = rating,

                Search = search,

                TotalReviews = evaluations.Count,

                AverageRating = evaluations.Any()
                    ? evaluations.Average(e => (double)e.rating)
                    : 0,

                FiveStarReviews = evaluations.Count(
                    e => e.rating == 5)
            };

            return View(model);
        }

        // POST: /Admin/Evaluations/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var evaluation = await _context.Evaluations
                .FirstOrDefaultAsync(
                    e => e.EvaluationId == id);

            if (evaluation == null)
            {
                return NotFound();
            }

            _context.Evaluations.Remove(evaluation);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }

    // Dashboard ViewModel
    public class EvaluationDashboardViewModel
    {
        public List<Evaluation> Evaluations { get; set; }
            = new List<Evaluation>();

        public int TotalReviews { get; set; }

        public double AverageRating { get; set; }

        public int FiveStarReviews { get; set; }

        public int? SelectedRating { get; set; }

        public string? Search { get; set; }
    }
}
