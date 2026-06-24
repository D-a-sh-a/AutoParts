using AutoParts.Enums;

namespace AutoParts.ViewModels
{
	public class CancelOrderViewModel
	{
		public int OrderId { get; set; }
		public CancelReason Reason { get; set; }
		public string? CustomReason { get; set; }
	}

	public class ShipOrderViewModel
	{
		public int OrderId { get; set; }
		public string TrackingNumber { get; set; } = null!;
	}
}