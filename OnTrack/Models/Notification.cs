using System.ComponentModel.DataAnnotations.Schema;

namespace OnTrack.Models
{
    public class Notification
    {
        public int NotificationId { get; set; }
        public string Message { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }

        [ForeignKey(nameof(ApplicationUser))]
        public string UserId { get; set; }

        [ForeignKey(nameof(Trip))]
        public int? TripId { get; set; }
        public ApplicationUser User { get; set; }

        public Trip? Trip { get; set; }
    }
}
