namespace AutoParts.Models
{
	public class AutoPart
	{
		public int Id { get; set; }
		public string SKU { get; set; } = string.Empty;
		public string Name { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public decimal Price { get; set; }
		public List<string> ImageUrls { get; set; } = new List<string>();
		public int StockQuantity { get; set; }
		public int? BrandId { get; set; }
		public Brand? Brand { get; set; }

		public int CategoryId { get; set; }
		public Category? Category { get; set; }
		public List<Vehicle> Vehicles { get; set; } = new();
	}
}