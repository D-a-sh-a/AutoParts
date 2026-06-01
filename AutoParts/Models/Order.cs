using AutoParts.Entities;
using AutoParts.Enums;
using Microsoft.EntityFrameworkCore;
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

		public int UserId { get; set; }
		public DbUser? User { get; set; }

		public List<OrderItem> OrderItems { get; set; } = new();
	}
}