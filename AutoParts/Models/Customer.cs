using System.ComponentModel.DataAnnotations;
using AutoParts.Entities;

namespace AutoParts.Models
{
	public class Customer
	{
		[Key]
		public int Id { get; set; }

		[Required]
		public string FirstName { get; set; } = null!;

		[Required]
		public string LastName { get; set; } = null!;

		[Required]
		public string Email { get; set; } = null!;

		[Required]
		public string Phone { get; set; } = null!;

		public int? UserId { get; set; }
		public virtual DbUser? User { get; set; }

		public List<Order> Orders { get; set; } = new();
	}
}