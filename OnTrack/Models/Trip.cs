using System.ComponentModel.DataAnnotations.Schema;

namespace OnTrack.Models
{
    public enum TripStatus
    {
        Scheduled,
        OnTime,
        Delayed,
        Completed,
        Cancelled
    }
    public class Trip
    {
        public int TripId { get; set; }
        public string? RouteName { get; set; }
        public string Status { get; set; }
        public DateTime TripDate { get; set; }

        [ForeignKey(nameof(Train))]
        public int TrainId { get; set; }
        public Train Train { get; set; }
        public ICollection<TripStop> TripStops { get; set; }

        public ICollection<Ticket> Tickets { get; set; }

        public ICollection<TripLocation> TripLocations { get; set; }

        public ICollection<Notification> Notifications { get; set; }

    }
}
