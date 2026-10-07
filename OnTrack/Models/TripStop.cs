using System.ComponentModel.DataAnnotations.Schema;

namespace OnTrack.Models
{
    public class TripStop
    {
        public int TripStopId { get; set; }
        public int StopOrder { get; set; }
        public DateTime? ScheduledTime { get; set; }
        public DateTime? ActualTime { get; set; }
        public string Platform { get; set; }
        [ForeignKey(nameof(Trip))]
        public int TripId { get; set; }

        [ForeignKey(nameof(Station))]
        public int StationId { get; set; }
        public Trip Trip { get; set; }
        public Station Station { get; set; }

    }
}
