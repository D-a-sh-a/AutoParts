using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AutoParts.Models
{
	public class Vehicle
	{
		public int Id { get; set; }

		[Range(1990, 2026, ErrorMessage = "Рік має бути від 1990 до 2026")]
		public int Year { get; set; }
		public int MakeId { get; set; }
		public Make? Make { get; set; }

		public int ModelId { get; set; }
		public Model? Model { get; set; }

		public int BodyTypeId { get; set; }
		public BodyType? BodyType { get; set; }

		public int EngineId { get; set; }
		public Engine? Engine { get; set; }
		public List<AutoPart> AutoParts { get; set; } = new();
	}
}