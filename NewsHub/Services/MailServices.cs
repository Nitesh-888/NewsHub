using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
namespace NewsHub.Services
{
    public class MailServices
    {
        private readonly IConfiguration _config;

        public MailServices(IConfiguration configuration)
        {
            _config = configuration;
        }

        public async Task SendEmail(string toEmail, string recipientName, string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("NewsHub", _config["MailSettings:Mail"]));
            message.To.Add(new MailboxAddress(recipientName, toEmail));
            message.Subject = subject;

            message.Body = new TextPart("html")
            {
                Text = body
            };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_config["MailSettings:SmtpServer"], 587, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_config["MailSettings:Username"], _config["MailSettings:Password"]);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }

        //method to send otp email
        public async Task SendOtpEmail(string toEmail, string recipientName, string otp)
        {
            string subject = "Your OTP Code";
            string body = $"<p>Dear {recipientName},</p>" +
                          $"<p>Your OTP code is: <strong>{otp}</strong></p>" +
                          "<p>This code is valid for 10 minutes.</p>" +
                          "<p>If you did not request this code, please ignore this email.</p>" +
                          "<br><p>Best regards,<br>NewsHub Team</p>";

            await SendEmail(toEmail, recipientName, subject, body);
        }
    }
}
