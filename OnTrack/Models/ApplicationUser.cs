using Microsoft.AspNetCore.Identity;

namespace OnTrack.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;

    }
}
