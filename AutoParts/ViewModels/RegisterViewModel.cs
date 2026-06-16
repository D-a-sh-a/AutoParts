using System.ComponentModel.DataAnnotations;

namespace AutoParts.ViewModels
{
	public class RegisterViewModel
	{
		[Required(ErrorMessage = "Ім'я є обов'язковим")]
		[Display(Name = "Ім'я")]
		public string FirstName { get; set; } = null!;

		[Required(ErrorMessage = "Прізвище є обов'язковим")]
		[Display(Name = "Прізвище")]
		public string LastName { get; set; } = null!;

		[Required(ErrorMessage = "Електронна пошта є обов'язковою")]
		[EmailAddress(ErrorMessage = "Некоректний формат Email")]
		[Display(Name = "Email")]
		public string Email { get; set; } = null!;

		[Required(ErrorMessage = "Телефон є обов'язковим")]
		[Phone(ErrorMessage = "Некоректний формат телефону")]
		[Display(Name = "Телефон")]
		public string Phone { get; set; } = null!;

		[Required(ErrorMessage = "Пароль є обов'язковим")]
		[StringLength(100, ErrorMessage = "Пароль має бути не менше {2} символів.", MinimumLength = 6)]
		[DataType(DataType.Password)]
		[Display(Name = "Пароль")]
		public string Password { get; set; } = null!;

		[DataType(DataType.Password)]
		[Display(Name = "Підтвердження пароля")]
		[Compare("Password", ErrorMessage = "Паролі не співпадають.")]
		public string ConfirmPassword { get; set; } = null!;
	}
}