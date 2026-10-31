
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnTrack.Data;
using OnTrack.Models;

namespace OnTrack.Controllers
{
    [Authorize]
    public class EvaluationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public EvaluationsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(
            int? rating,
            string? search)
        {
            var userId = _userManager.GetUserId(User);

            var query = _context.Evaluations
                .Include(e => e.Ticket)
                .Where(e => e.Ticket.UserId == userId);

            if (rating.HasValue && rating.Value >= 1
                && rating.Value <= 5)
            {
                query = query.Where(
                    e => e.rating == rating.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(e =>
                    (e.comment != null &&
                     e.comment.Contains(search)) ||
                    e.TicketId.ToString().Contains(search));
            }

            var evaluations = await query
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();

            var model = new EvaluationDashboardViewModel
            {
                Evaluations = evaluations,
                SelectedRating = rating,
                Search = search,
                TotalReviews = evaluations.Count,
                AverageRating = evaluations.Count > 0
                    ? evaluations.Average(e => e.rating)
                    : 0,
                FiveStarReviews = evaluations.Count(
                    e => e.rating == 5)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);

            var evaluation = await _context.Evaluations
                .Include(e => e.Ticket)
                .FirstOrDefaultAsync(e =>
                    e.EvaluationId == id &&
                    e.Ticket.UserId == userId);

            if (evaluation == null)
            {
                return NotFound();
            }

            _context.Evaluations.Remove(evaluation);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }


}
