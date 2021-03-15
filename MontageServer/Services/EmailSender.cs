using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MontageServer.Services;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Net.Mail;
using System.Threading.Tasks;


namespace WebPWrecover.Services
{
    public class EmailSender : IEmailSender
    {
        public EmailSender(IOptions<AuthMessageSenderOptions> optionsAccessor)
        {
            Options = optionsAccessor.Value;
        }

        public AuthMessageSenderOptions Options { get; } //set only via Secret Manager

        public Task SendEmailAsync(string email, string subject, string message)
        {
            return Execute(Options.SendGridKey, subject, message, email);
        }

        public Task Execute(string apiKey, string subject, string message, string email)
        {
            /*
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("montageextension@gmail.com");
            mailMessage.To.Add(new MailAddress(email));

            mailMessage.Subject = "Password Reset";
            mailMessage.IsBodyHtml = true;
            mailMessage.Body = message;

            SmtpClient client = new SmtpClient();
            client.Credentials = new System.Net.NetworkCredential("montageextension@gmail.com", Options.SendGridUser);
            client.Host = "smtp.sendgrid.net";
            client.Port = 587;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.Send(mailMessage);
            */



            var client2 = new SendGridClient(apiKey);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress("montageextension@gmail.com", Options.SendGridUser),
                Subject = subject,
                PlainTextContent = message,
                HtmlContent = message
            };
            msg.AddTo(new EmailAddress(email));

            // Disable click tracking.
            // See https://sendgrid.com/docs/User_Guide/Settings/tracking.html
            msg.SetClickTracking(false, false);

            return client2.SendEmailAsync(msg);
            
        }
    }
}