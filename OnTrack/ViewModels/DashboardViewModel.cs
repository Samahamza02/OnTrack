
namespace OnTrack.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalUsers { get; set; }

        public List<DashboardStatistic> Statistics { get; set; } = new();
    }

    public class DashboardStatistic
    {
        public string Name { get; set; } = string.Empty;

        public int Count { get; set; }
    }
}
