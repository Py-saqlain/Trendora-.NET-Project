using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Trendora.Services
{
    public class RealEmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<RealEmailSender> _logger;

        public RealEmailSender(IConfiguration configuration, ILogger<RealEmailSender> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                var smtpSettings = _configuration.GetSection("SmtpSettings");

                var fromEmail = smtpSettings["FromEmail"] ?? "noreply@trendora.com";
                var fromName = smtpSettings["FromName"] ?? "Trendora Support";

                // Create mail message
                var mail = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true
                };
                mail.To.Add(email);

                // Create SMTP client
                using var smtpClient = new SmtpClient(smtpSettings["Host"])
                {
                    Port = int.Parse(smtpSettings["Port"] ?? "587"),
                    Credentials = new NetworkCredential(
                        smtpSettings["Username"],
                        smtpSettings["Password"]
                    ),
                    EnableSsl = bool.Parse(smtpSettings["EnableSsl"] ?? "true")
                };

                await smtpClient.SendMailAsync(mail);

                _logger.LogInformation($"Email sent successfully to {email}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {email}");
                throw;
            }
        }
    }
}