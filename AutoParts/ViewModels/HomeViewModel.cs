using AutoParts.Models;

namespace AutoParts.ViewModels
{
	public class HomeViewModel
	{
		public List<int> AvailableYears { get; set; } = new List<int>();
		public List<Category> Categories { get; set; } = new List<Category>();
		public List<AutoPart> FeaturedParts { get; set; } = new List<AutoPart>();
		public List<Brand> Brands { get; set; } = new List<Brand>();
	}
}