using AutoParts.ViewModels;
using AutoParts.Data;
using AutoParts.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace AutoParts.Controllers
{
	public class HomeController : Controller
	{
		private readonly ApplicationDbContext _context;

		public HomeController(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task<IActionResult> Index()
		{
			var vm = new HomeViewModel();

			vm.AvailableYears = await _context.Vehicles
				.Select(v => v.Year)
				.Distinct()
				.OrderByDescending(y => y)
				.ToListAsync();

			vm.Categories = await _context.Categories.ToListAsync();

			vm.FeaturedParts = await _context.AutoParts
				.Include(p => p.Category)
				.Include(p => p.Brand)
				.Take(10)
				.ToListAsync();

			vm.Brands = await _context.Brands.Take(10).ToListAsync();

			return View(vm);
		}


		[HttpGet]
		public async Task<JsonResult> GetMakes(int year)
		{
			var makes = await _context.Vehicles
				.Where(v => v.Year == year)
				.Select(v => new { id = v.MakeId, name = v.Make!.Name })
				.Distinct()
				.OrderBy(m => m.name)
				.ToListAsync();
			return Json(makes);
		}

		[HttpGet]
		public async Task<JsonResult> GetModels(int year, int makeId)
		{
			var models = await _context.Vehicles
				.Where(v => v.Year == year && v.MakeId == makeId)
				.Select(v => new { id = v.ModelId, name = v.Model!.Name })
				.Distinct()
				.OrderBy(m => m.name)
				.ToListAsync();
			return Json(models);
		}

		[HttpGet]
		public async Task<JsonResult> GetBodyTypes(int year, int makeId, int modelId)
		{
			var bodies = await _context.Vehicles
				.Where(v => v.Year == year && v.MakeId == makeId && v.ModelId == modelId)
				.Select(v => new { id = v.BodyTypeId, name = v.BodyType!.Name })
				.Distinct()
				.OrderBy(b => b.name)
				.ToListAsync();
			return Json(bodies);
		}

		[HttpGet]
		public async Task<JsonResult> GetEngines(int year, int makeId, int modelId, int bodyId)
		{
			var engines = await _context.Vehicles
				.Where(v => v.Year == year && v.MakeId == makeId && v.ModelId == modelId && v.BodyTypeId == bodyId)
				.Select(v => new { id = v.EngineId, name = v.Engine!.Name, vehicleId = v.Id })
				.Distinct()
				.OrderBy(e => e.name)
				.ToListAsync();
			return Json(engines);
		}


		public IActionResult Catalog(int vehicleId)
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}