using System.Net;
using System.Net.Mail;

namespace EduQuest.API.Services
{
    public interface IEmailService
    {
        Task SendAsync(string toEmail, string subject, string htmlBody);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendAsync(string toEmail, string subject, string htmlBody)
        {
            var host = _config["Email:Host"];

            // No SMTP server configured (e.g. local development):
            // print the email to the API console so you can still test.
            if (string.IsNullOrWhiteSpace(host))
            {
                _logger.LogWarning(
                    "Email not configured. Would have sent to {To}: {Subject}\n{Body}",
                    toEmail, subject, htmlBody);
                return;
            }

            var port = int.Parse(_config["Email:Port"] ?? "587");
            var user = _config["Email:User"];
            var password = _config["Email:Password"];
            var from = _config["Email:From"] ?? user!;
            var enableSsl = bool.Parse(_config["Email:EnableSsl"] ?? "true");

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = enableSsl,
                Credentials = new NetworkCredential(user, password)
            };

            using var message = new MailMessage(from, toEmail, subject, htmlBody)
            {
                IsBodyHtml = true
            };

            await client.SendMailAsync(message);
        }
    }
}