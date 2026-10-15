using Microsoft.EntityFrameworkCore;
using OnTrack.Data;
using OnTrack.ViewModels;

namespace OnTrack.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _context;

        public DashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardViewModel> GetDashboardStatisticsAsync()
        {
            var model = new DashboardViewModel
            {
                TotalUsers = await _context.Users.CountAsync(),

                Statistics = new List<DashboardStatistic>
                {
                    new()
                    {
                        Name = "Trips",
                        Count = await _context.Trips.CountAsync()
                    },
                    new()
                    {
                        Name = "Tickets",
                        Count = await _context.Tickets.CountAsync()
                    },
                    new()
                    {
                        Name = "Trains",
                        Count = await _context.Trains.CountAsync()
                    },
                    new()
                    {
                        Name = "Trip Stops",
                        Count = await _context.TripStops.CountAsync()
                    },
                    new()
                    {
                        Name = "Trip Locations",
                        Count = await _context.TripLocations.CountAsync()
                    },
                    new()
                    {
                        Name = "Notifications",
                        Count = await _context.Notifications.CountAsync()
                    },
                    new()
                    {
                        Name = "Evaluations",
                        Count = await _context.Evaluations.CountAsync()
                    },
                    new()
                    {
                        Name = "Stations",
                        Count = await _context.Stations.CountAsync()
                    },
                    new()
                    {
                        Name = "Payments",
                        Count = await _context.Payments.CountAsync()
                    }
                }
            };

            return model;
        }
    }
}
