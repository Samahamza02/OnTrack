using Microsoft.AspNetCore.Identity;

namespace OnTrack.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
