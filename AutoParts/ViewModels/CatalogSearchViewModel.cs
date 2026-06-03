using AutoParts.Models;

namespace AutoParts.ViewModels
{
	public class CatalogSearchViewModel
	{
		public Vehicle? ActiveVehicle { get; set; }
		public List<AutoPart> Parts { get; set; } = new();
		public List<Category> Categories { get; set; } = new();
		public List<Brand> Brands { get; set; } = new();
		public List<int> AvailableYears { get; set; } = new();
		public decimal CatalogMinPrice { get; set; }
		public decimal CatalogMaxPrice { get; set; }

		public int? SelectedVehicleId { get; set; }
		public decimal? SelectedMinPrice { get; set; }
		public decimal? SelectedMaxPrice { get; set; }
		public List<int> SelectedCategories { get; set; } = new();
		public List<int> SelectedBrands { get; set; } = new();
		public string? SelectedSortOrder { get; set; }
	}
}