
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnTrack.Services;

namespace OnTrack.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public async Task<IActionResult> Index()
        {
            var dashboard =
                await _dashboardService.GetDashboardStatisticsAsync();

            return View(dashboard);
        }
    }
}
