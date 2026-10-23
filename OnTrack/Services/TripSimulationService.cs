
using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using OnTrack.Data;
using OnTrack.Hubs;
using OnTrack.Models;

namespace OnTrack.Services
{
    public class TripSimulationService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHubContext<TripTrackingHub> _hubContext;
        private readonly ConcurrentDictionary<int, byte> _runningTrips = new();

        public TripSimulationService(
            IServiceScopeFactory scopeFactory,
            IHubContext<TripTrackingHub> hubContext)
        {
            _scopeFactory = scopeFactory;
            _hubContext = hubContext;
        }

        public bool StartSimulation(int tripId)
        {
            if (!_runningTrips.TryAdd(tripId, 0))
                return false;

            // Queue the trip for the background service.
            if (!_queue.Writer.TryWrite(tripId))
            {
                _runningTrips.TryRemove(tripId, out _);
                return false;
            }

            return true;
        }

        private readonly System.Threading.Channels.Channel<int> _queue =
            System.Threading.Channels.Channel.CreateUnbounded<int>();

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (int tripId in _queue.Reader.ReadAllAsync(stoppingToken))
            {
                _ = SimulateTripAsync(tripId, stoppingToken);
            }
        }

        private async Task SimulateTripAsync(
            int tripId,
            CancellationToken stoppingToken)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();

                var context = scope.ServiceProvider
                    .GetRequiredService<ApplicationDbContext>();

                var trip = await context.Trips
                    .Include(t => t.TripStops)
                    .ThenInclude(stop => stop.Station)
                    .FirstOrDefaultAsync(
                        t => t.TripId == tripId,
                        stoppingToken);

                if (trip == null ||
                    trip.Status == TripStatus.Cancelled ||
                    trip.Status == TripStatus.Completed)
                {
                    return;
                }

                var stops = trip.TripStops
                    .Where(s => s.ScheduledTime.HasValue && s.Station != null)
                    .OrderBy(s => s.StopOrder)
                    .ToList();

                if (stops.Count < 2)
                    return;

                // Combine the trip date with each stop's scheduled time.
                var schedule = new List<StopSchedule>();
                DateTime? previousTime = null;

                foreach (var stop in stops)
                {
                    DateTime time = trip.TripDate.Date
                        .Add(stop.ScheduledTime!.Value.TimeOfDay);

                    // If the next stop's time is earlier, it is on the next day.
                    if (previousTime.HasValue && time < previousTime.Value)
                        time = time.AddDays(1);

                    schedule.Add(new StopSchedule(
                        stop.Station!.StationName,
                        (double)stop.Station.Latitude,
                        (double)stop.Station.Longitude,
                        time));

                    previousTime = time;
                }

                while (!stoppingToken.IsCancellationRequested)
                {
                    DateTime now = DateTime.Now;

                    var position = CalculatePosition(schedule, now);

                    if (position.Completed)
                    {
                        trip.Status = TripStatus.Completed;
                    }

                    context.TripLocations.Add(new TripLocation
                    {
                        TripId = tripId,
                        Latitude = (decimal)position.Latitude,
                        Longitude = (decimal)position.Longitude,
                        Speed = (decimal)position.Speed,
                        RecordedAt = now
                    });

                    await context.SaveChangesAsync(stoppingToken);

                    await _hubContext.Clients
                        .Group($"Trip-{tripId}")
                        .SendAsync("ReceiveLocation", new
                        {
                            tripId,
                            latitude = position.Latitude,
                            longitude = position.Longitude,
                            speed = position.Speed,
                            recordedAt = now,
                            status = trip.Status.ToString(),
                            nextStation = position.NextStation,
                            scheduledArrival = schedule[^1].Time
                        }, stoppingToken);

                    if (position.Completed)
                        break;

                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                // The application is shutting down.
            }
            finally
            {
                _runningTrips.TryRemove(tripId, out _);
            }
        }

        private static Position CalculatePosition(
            List<StopSchedule> stops,
            DateTime now)
        {
            var first = stops[0];
            var last = stops[^1];

            // Before departure: wait at the first station.
            if (now < first.Time)
            {
                return new Position(
                    first.Latitude,
                    first.Longitude,
                    0,
                    first.Name,
                    false);
            }

            // After arrival: remain at the final station.
            if (now >= last.Time)
            {
                return new Position(
                    last.Latitude,
                    last.Longitude,
                    0,
                    last.Name,
                    true);
            }

            for (int i = 0; i < stops.Count - 1; i++)
            {
                var from = stops[i];
                var to = stops[i + 1];

                if (now >= from.Time && now < to.Time)
                {
                    double totalSeconds =
                        (to.Time - from.Time).TotalSeconds;

                    double elapsedSeconds =
                        (now - from.Time).TotalSeconds;

                    double progress = totalSeconds <= 0
                        ? 1
                        : Math.Clamp(elapsedSeconds / totalSeconds, 0, 1);

                    double latitude = from.Latitude +
                        (to.Latitude - from.Latitude) * progress;

                    double longitude = from.Longitude +
                        (to.Longitude - from.Longitude) * progress;

                    double distanceKm = DistanceKm(
                        from.Latitude, from.Longitude,
                        to.Latitude, to.Longitude);

                    double durationHours =
                        (to.Time - from.Time).TotalHours;

                    double speed = durationHours > 0
                        ? distanceKm / durationHours
                        : 0;

                    return new Position(
                        latitude,
                        longitude,
                        speed,
                        to.Name,
                        false);
                }
            }

            return new Position(
                last.Latitude,
                last.Longitude,
                0,
                last.Name,
                now >= last.Time);
        }

        private static double DistanceKm(
            double lat1, double lon1,
            double lat2, double lon2)
        {
            const double earthRadiusKm = 6371;

            double dLat = DegreesToRadians(lat2 - lat1);
            double dLon = DegreesToRadians(lon2 - lon1);

            double a =
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(DegreesToRadians(lat1)) *
                Math.Cos(DegreesToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            return earthRadiusKm * 2 *
                Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        }

        private static double DegreesToRadians(double degrees)
            => degrees * Math.PI / 180;

        private sealed record StopSchedule(
            string Name,
            double Latitude,
            double Longitude,
            DateTime Time);

        private sealed record Position(
            double Latitude,
            double Longitude,
            double Speed,
            string NextStation,
            bool Completed);
    }
}
