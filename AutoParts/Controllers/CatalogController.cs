using Microsoft.AspNetCore.Mvc;

namespace AutoParts.Controllers
{
	public class CatalogController : Controller
	{
		[HttpGet]
		public IActionResult Search()
		{
			return View();
		}
	}
}
