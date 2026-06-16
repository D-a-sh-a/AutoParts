using System.ComponentModel.DataAnnotations;
using AutoParts.Models;

namespace AutoParts.ViewModels
{
	public class CheckoutViewModel
	{
		[Required(ErrorMessage = "Введіть ваше ім'я")]
		[Display(Name = "Ім'я")]
		public string FirstName { get; set; } = null!;

		[Required(ErrorMessage = "Введіть ваше прізвище")]
		[Display(Name = "Прізвище")]
		public string LastName { get; set; } = null!;

		[Required(ErrorMessage = "Введіть електронну пошту")]
		[EmailAddress(ErrorMessage = "Невірний формат пошти")]
		[Display(Name = "Електронна пошта")]
		public string Email { get; set; } = null!;

		[Required(ErrorMessage = "Введіть номер телефону")]
		[Display(Name = "Телефон")]
		public string Phone { get; set; } = null!;

		[Required(ErrorMessage = "Введіть адресу доставки")]
		[Display(Name = "Адреса доставки")]
		public string Address { get; set; } = null!;

		public string? Comment { get; set; }

		public List<CartItem> CartItems { get; set; } = new();
		public decimal TotalAmount { get; set; }
	}
}