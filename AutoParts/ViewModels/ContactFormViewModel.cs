using System.ComponentModel.DataAnnotations;

namespace AutoParts.ViewModels
{
	public class ContactFormViewModel
	{
		[Required(ErrorMessage = "Будь ласка, введіть ваше ім'я")]
		public string Name { get; set; }

		[Required(ErrorMessage = "Будь ласка, введіть ваш Email")]
		[EmailAddress(ErrorMessage = "Введено некоректний Email")]
		public string Email { get; set; }

		[Required(ErrorMessage = "Будь ласка, напишіть повідомлення")]
		public string Message { get; set; }
	}
}