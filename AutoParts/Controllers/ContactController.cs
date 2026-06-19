using AutoParts.Services;
using AutoParts.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace AutoParts.Controllers
{
	public class ContactController : Controller
	{
		private readonly EmailService _emailService;

		public ContactController(EmailService emailService)
		{
			_emailService = emailService;
		}

		[HttpGet]
		public IActionResult Index()
		{
			return View("~/Views/Home/Contacts.cshtml");
		}

		[HttpPost]
		public async Task<IActionResult> Index(ContactFormViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View("~/Views/Home/Contacts.cshtml", model);
			}

			try
			{
				string toEmail = "burian_ak21@nuwm.edu.ua";
				string subject = $"Нове повідомлення від {model.Name}";
				string body = $"Ім'я: {model.Name}<br/>Email: {model.Email}<br/><br/>Повідомлення:<br/>{model.Message}";

				await _emailService.SendEmailAsync(toEmail, subject, body);

				TempData["SuccessMessage"] = "Дякуємо! Ваше повідомлення успішно відправлено.";
				return RedirectToAction("Index");
			}
			catch (Exception ex)
			{
				ModelState.AddModelError("", "Вибачте, झालीся помилка при відправці: " + ex.Message);
				return View("~/Views/Home/Contacts.cshtml", model);
			}
		}
	}
}