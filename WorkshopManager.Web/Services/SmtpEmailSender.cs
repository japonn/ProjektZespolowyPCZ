using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using WorkshopManager.Web.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace WorkshopManager.Web.Services
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly EmailSettings _settings;

        public SmtpEmailSender(IOptions<EmailSettings> options)
        {
            _settings = options.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("", _settings.From));
            message.To.Add(new MailboxAddress("", email));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlMessage
            };
            message.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                // Automatycznie wybierz odpowiedni tryb SSL/TLS na podstawie portu
                var secureSocketOptions = _settings.Port == 465
                    ? SecureSocketOptions.SslOnConnect  // Port 465: SSL od początku
                    : SecureSocketOptions.StartTls;      // Port 587: STARTTLS

                await client.ConnectAsync(_settings.Host, _settings.Port, secureSocketOptions);
                await client.AuthenticateAsync(_settings.UserName, _settings.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }
    }
}