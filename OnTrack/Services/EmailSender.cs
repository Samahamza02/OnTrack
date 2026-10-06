using System.Net;
using System.Net.Mail;

namespace OnTrack.Services
{
    // Reads credentials from configuration (appsettings / User Secrets) -
    // never hardcode real credentials directly in this file.
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
            var fromEmail = _configuration["EmailSettings:FromEmail"];
            var appPassword = _configuration["EmailSettings:AppPassword"];

            using var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail, appPassword)
            };

            using var message = new MailMessage(fromEmail!, toEmail, subject, htmlMessage)
            {
                IsBodyHtml = true
            };

            await client.SendMailAsync(message);
        }
    }
}
