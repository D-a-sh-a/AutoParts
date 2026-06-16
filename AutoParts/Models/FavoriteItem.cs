using System.ComponentModel.DataAnnotations;
using AutoParts.Entities;

namespace AutoParts.Models
{
	public class FavoriteItem
	{
		[Key]
		public int Id { get; set; }

		public int AutoPartId { get; set; }
		public virtual AutoPart? AutoPart { get; set; }

		public int UserId { get; set; }
		public virtual DbUser? User { get; set; }

		public DateTime DateAdded { get; set; } = DateTime.UtcNow;
	}
}