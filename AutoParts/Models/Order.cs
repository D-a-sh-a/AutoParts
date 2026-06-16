using AutoParts.Enums;
using System;
using System.Collections.Generic;

namespace AutoParts.Models
{
	public class Order
	{
		public int Id { get; set; }
		public DateTime OrderDate { get; set; } = DateTime.Now;
		public decimal TotalAmount { get; set; }
		public OrderStatus Status { get; set; } = OrderStatus.Pending;

		public string ShippingAddress { get; set; } = null!;
		public string? Comment { get; set; }

		public int CustomerId { get; set; }
		public virtual Customer? Customer { get; set; }

		public List<OrderItem> OrderItems { get; set; } = new();
	}
}