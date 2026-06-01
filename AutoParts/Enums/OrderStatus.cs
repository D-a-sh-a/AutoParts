using System.ComponentModel.DataAnnotations;

namespace AutoParts.Enums
{
	public enum OrderStatus
	{
		[Display(Name = "В очікуванні")]
		Pending,

		[Display(Name = "В обробці")]
		Processing,

		[Display(Name = "Відправлено")]
		Shipped,

		[Display(Name = "Виконано")]
		Completed,

		[Display(Name = "Скасовано")]
		Cancelled
	}
}