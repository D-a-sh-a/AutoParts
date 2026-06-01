using Microsoft.AspNetCore.Identity;

namespace AutoParts.Entities
{
	public class DbUser : IdentityUser<int>
	{
		public string FirstName { get; set; } = string.Empty;
		public string LastName { get; set; } = string.Empty;
	}
}