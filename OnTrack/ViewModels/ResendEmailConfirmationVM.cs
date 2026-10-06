using System.ComponentModel.DataAnnotations;

namespace OnTrack.ViewModels
{
    public class ResendEmailConfirmationVM
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
