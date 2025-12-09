using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using WorkshopManager.Web.Settings;
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
            using (var client = new SmtpClient(_settings.Host, _settings.Port))
            {
                client.EnableSsl = _settings.EnableSsl;
                client.Credentials = new NetworkCredential(_settings.UserName, _settings.Password);

                var mail = new MailMessage
                {
                    From = new MailAddress(_settings.From),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true
                };

                mail.To.Add(email);
                await client.SendMailAsync(mail);
            }
        }
    }
}