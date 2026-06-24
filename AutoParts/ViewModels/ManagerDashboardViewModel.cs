using System;
using System.Collections.Generic;

namespace AutoParts.ViewModels
{
	public class ManagerDashboardViewModel
	{
		public int NewOrdersCount { get; set; }
		public int TotalPartsCount { get; set; }
		public int LowStockCount { get; set; }

		public List<ManagerDashboardOrderViewModel> RecentOrders { get; set; } = new();
	}

	public class ManagerDashboardOrderViewModel
	{
		public int Id { get; set; }
		public string CustomerName { get; set; } = null!;
		public DateTime OrderDate { get; set; }
		public decimal TotalSum { get; set; }
		public string Status { get; set; } = null!;
	}
}