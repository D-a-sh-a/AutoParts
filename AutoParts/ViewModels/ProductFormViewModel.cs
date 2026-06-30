using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AutoParts.ViewModels
{
	public class ProductFormViewModel
	{
		public int Id { get; set; }
		public string? SKU { get; set; }

		[Required(ErrorMessage = "Назва товару є обов'язковою")]
		[StringLength(200)]
		public string Name { get; set; } = null!;

		[Required(ErrorMessage = "Ціна є обов'язковою")]
		[Range(0.01, 1000000, ErrorMessage = "Ціна повинна бути більшою за 0")]
		public decimal Price { get; set; }

		[Required(ErrorMessage = "Кількість є обов'язковою")]
		[Range(0, 100000, ErrorMessage = "Кількість не може бути від'ємною")]
		public int StockQuantity { get; set; }

		public string? Description { get; set; }
		public List<int> SelectedVehicleIds { get; set; } = new List<int>();

		[Required(ErrorMessage = "Оберіть категорію")]
		public int CategoryId { get; set; }

		[Required(ErrorMessage = "Оберіть бренд")]
		public int BrandId { get; set; }

		public List<IFormFile>? ImageFiles { get; set; }

		public List<string>? ExistingImageUrls { get; set; }
	}
}