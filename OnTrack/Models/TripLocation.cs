using System.ComponentModel.DataAnnotations.Schema;

namespace OnTrack.Models
{
    public class TripLocation
    {
        public int TripLocationId { get; set; }
        [ForeignKey(nameof(Trip))]
        public int TripId { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public decimal Speed { get; set; }
        public DateTime RecordedAt { get; set; }
        public Trip Trip { get; set; }

    }
}
