namespace AutoParts.Models
{
	public class CartItem
	{
		public int Id { get; set; }

		public string CartId { get; set; }

		public int AutoPartId { get; set; }
		public AutoPart AutoPart { get; set; }

		public int Quantity { get; set; }
	}
}