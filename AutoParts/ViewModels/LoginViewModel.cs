using System.ComponentModel.DataAnnotations;

namespace AutoParts.ViewModels
{
	public class LoginViewModel
	{
		[Required(ErrorMessage = "Електронна пошта є обов'язковою")]
		[EmailAddress(ErrorMessage = "Некоректний формат Email")]
		[Display(Name = "Email")]
		public string Email { get; set; } = null!;

		[Required(ErrorMessage = "Пароль є обов'язковим")]
		[DataType(DataType.Password)]
		[Display(Name = "Пароль")]
		public string Password { get; set; } = null!;

		[Display(Name = "Запам'ятати мене")]
		public bool RememberMe { get; set; }
	}
}