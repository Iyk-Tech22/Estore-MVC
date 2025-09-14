using Microsoft.AspNetCore.Identity.UI.Services;
using Mailtrap.Source.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Estore.Utility
{
    public class EmailSender : IEmailSender
    {
        private readonly MailTrapSettings _mailTrapSettings;
        public EmailSender(IOptions<MailTrapSettings> mailTrapSettings)
        {
            _mailTrapSettings = mailTrapSettings.Value;
        }
        public Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
            var username = _mailTrapSettings.Username;
            var password = _mailTrapSettings.Password;
            var fromEmail = _mailTrapSettings.FromEmail;

            var mailtrap = new MailtrapSender(username, password);

            var email = new Email(
                toEmail,
                fromEmail,
                subject,
                htmlMessage,
                isBodyHtml:true
            );

           return  mailtrap.SendAsync(email);
        }
    }
}
