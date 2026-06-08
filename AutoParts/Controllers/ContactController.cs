using AutoParts.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;

namespace AutoParts.Controllers
{
	public class ContactController : Controller
	{
		[HttpGet]
		public IActionResult Index()
		{
			return View("~/Views/Home/Contacts.cshtml");
		}

		[HttpPost]
		public IActionResult Index(ContactFormViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View("~/Views/Home/Contacts.cshtml", model);
			}

			try
			{
				string fromEmail = "burian_ak21@nuwm.edu.ua";
				string appPassword = "kvnfqouxzwmduror";
				string toEmail = "burian_ak21@nuwm.edu.ua";

				MailMessage message = new MailMessage();
				message.From = new MailAddress(fromEmail, "Autoparts Website");
				message.To.Add(toEmail);
				message.Subject = $"Нове повідомлення від {model.Name}";
				message.Body = $"Ім'я: {model.Name}\nEmail: {model.Email}\n\nПовідомлення:\n{model.Message}";
				message.IsBodyHtml = false;

				using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
				{
					smtp.Credentials = new NetworkCredential(fromEmail, appPassword);
					smtp.EnableSsl = true;
					smtp.Send(message);
				}

				TempData["SuccessMessage"] = "Дякуємо! Ваше повідомлення успішно відправлено.";
				return RedirectToAction("Index");
			}
			catch (Exception ex)
			{
				ModelState.AddModelError("", "Вибачте, сталася помилка при відправці: " + ex.Message);
				return View("~/Views/Home/Contacts.cshtml", model);
			}
		}
	}
}