using OnTrack.ViewModels;

namespace OnTrack.Services
{
    public interface IDashboardService
    {
        Task<DashboardViewModel> GetDashboardStatisticsAsync();
    }
}
