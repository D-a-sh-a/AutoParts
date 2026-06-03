using Microsoft.AspNetCore.Mvc;

namespace AutoParts.Controllers
{
	public class ContactController : Controller
	{
		[HttpGet]
		public IActionResult Index()
		{
			return View("~/Views/Home/Contacts.cshtml");
		}

	}
}