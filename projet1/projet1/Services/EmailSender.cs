using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using projet1.Helpers;

namespace projet1.Services
{
    public class EmailSender : IAppEmailSender
    {
        private readonly EmailConfiguration _config;
        public EmailSender(EmailConfiguration config)
            => _config = config;

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(_config.From));
            message.To.Add(MailboxAddress.Parse(email));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = htmlMessage };

            using var client = new SmtpClient();
            await client.ConnectAsync(_config.SmtpServer, _config.Port, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_config.Username, _config.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
