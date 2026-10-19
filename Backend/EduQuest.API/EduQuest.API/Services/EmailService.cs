
using System.Net;
using System.Net.Mail;

namespace EduQuest.API.Services
{
    public interface IEmailService
    {
        Task SendAsync(
            string toEmail,
            string subject,
            string htmlBody
        );
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly IHostEnvironment _environment;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IConfiguration config,
            IHostEnvironment environment,
            ILogger<EmailService> logger)
        {
            _config = config;
            _environment = environment;
            _logger = logger;
        }

        public async Task SendAsync(
            string toEmail,
            string subject,
            string htmlBody)
        {
            var host = _config["Email:Host"];

            // Local development fallback only.
            // Do not expose reset tokens in production logs.
            if (string.IsNullOrWhiteSpace(host))
            {
                if (_environment.IsDevelopment())
                {
                    _logger.LogWarning(
                        "DEVELOPMENT ONLY: Email delivery is not configured. " +
                        "Use the reset link below from the local API console. " +
                        "To: {Recipient}; Subject: {Subject}; HTML: {HtmlBody}",
                        toEmail,
                        subject,
                        htmlBody
                    );

                    return;
                }

                throw new InvalidOperationException(
                    "SMTP is not configured. " +
                    "Configure Email:Host and mail credentials " +
                    "in the server environment."
                );
            }

            // Validate SMTP port.
            if (
                !int.TryParse(
                    _config["Email:Port"],
                    out var port
                ) ||
                port < 1 ||
                port > 65535
            )
            {
                throw new InvalidOperationException(
                    "Email:Port must be a valid SMTP port."
                );
            }

            var username = _config["Email:User"];
            var password = _config["Email:Password"];
            var from = _config["Email:From"];

            if (
                string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(from)
            )
            {
                throw new InvalidOperationException(
                    "SMTP credentials are missing. " +
                    "Configure Email:User, Email:Password " +
                    "and Email:From using secrets."
                );
            }

            var enableSsl =
                !bool.TryParse(
                    _config["Email:EnableSsl"],
                    out var ssl
                ) || ssl;

            // Create SMTP connection.
            using var client = new SmtpClient(host, port)
            {
                EnableSsl = enableSsl,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(
                    username,
                    password
                ),
                DeliveryMethod = SmtpDeliveryMethod.Network
            };

            // Create email.
            using var message = new MailMessage
            {
                From = new MailAddress(from),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            message.To.Add(new MailAddress(toEmail));

            // Send reset email.
            await client.SendMailAsync(message);
        }
    }
}
