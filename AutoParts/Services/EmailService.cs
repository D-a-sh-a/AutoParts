using System.Net;
using System.Net.Mail;

namespace AutoParts.Services
{
	public class EmailService
	{
		private readonly string _fromEmail = "burian_ak21@nuwm.edu.ua";
		private readonly string _appPassword = "kvnfqouxzwmduror";

		public async Task SendEmailAsync(string toEmail, string subject, string body)
		{
			var message = new MailMessage();
			message.From = new MailAddress(_fromEmail, "AUTOparts Store");
			message.To.Add(toEmail);
			message.Subject = subject;
			message.Body = body;
			message.IsBodyHtml = true;

			using (var smtp = new SmtpClient("smtp.gmail.com", 587))
			{
				smtp.Credentials = new NetworkCredential(_fromEmail, _appPassword);
				smtp.EnableSsl = true;

				await smtp.SendMailAsync(message);
			}
		}
	}
}