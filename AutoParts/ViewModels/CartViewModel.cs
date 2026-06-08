using AutoParts.Models;

namespace AutoParts.ViewModels
{
	public class CartViewModel
	{
		public List<CartItem> Items { get; set; } = new List<CartItem>();

		public decimal TotalSum => Items.Sum(item => item.AutoPart.Price * item.Quantity);
	}
}