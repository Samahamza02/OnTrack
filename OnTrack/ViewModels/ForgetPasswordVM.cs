using System.ComponentModel.DataAnnotations;

namespace OnTrack.ViewModels
{
    public class ForgetPasswordVM
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
