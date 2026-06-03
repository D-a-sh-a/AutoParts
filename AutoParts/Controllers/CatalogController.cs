using AutoParts.Data;
using AutoParts.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoParts.Controllers
{
	public class CatalogController : Controller
	{
		private readonly ApplicationDbContext _context;

		public CatalogController(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task<IActionResult> Search(int? vehicleId, decimal? minPrice, decimal? maxPrice, List<int> categories, List<int> brands, string sortOrder)
		{
			var viewModel = new CatalogSearchViewModel
			{
				SelectedVehicleId = vehicleId,
				SelectedMinPrice = minPrice,
				SelectedMaxPrice = maxPrice,
				SelectedCategories = categories ?? new List<int>(),
				SelectedBrands = brands ?? new List<int>(),
				SelectedSortOrder = sortOrder ?? "relevance"
			};

			viewModel.Categories = await _context.Categories.ToListAsync();
			viewModel.Brands = await _context.Brands.ToListAsync();
			viewModel.AvailableYears = await _context.Vehicles.Select(v => v.Year).Distinct().OrderByDescending(y => y).ToListAsync();

			if (vehicleId.HasValue)
			{
				viewModel.ActiveVehicle = await _context.Vehicles
					.Include(v => v.Make)
					.Include(v => v.Model)
					.Include(v => v.BodyType)
					.Include(v => v.Engine)
					.FirstOrDefaultAsync(v => v.Id == vehicleId.Value);
			}

			var query = _context.AutoParts
				.Include(p => p.Category)
				.Include(p => p.Brand)
				.AsQueryable();

			if (vehicleId.HasValue)
			{
				query = query.Where(p => p.Vehicles.Any(v => v.Id == vehicleId.Value));
			}

			if (categories != null && categories.Any())
			{
				query = query.Where(p => categories.Contains(p.CategoryId));
			}

			if (brands != null && brands.Any())
			{
				query = query.Where(p => p.BrandId.HasValue && brands.Contains(p.BrandId.Value));
			}

			if (await query.AnyAsync())
			{
				viewModel.CatalogMinPrice = await query.MinAsync(p => p.Price);
				viewModel.CatalogMaxPrice = await query.MaxAsync(p => p.Price);
			}
			else
			{
				viewModel.CatalogMinPrice = 0;
				viewModel.CatalogMaxPrice = 10000;
			}

			if (minPrice.HasValue)
			{
				query = query.Where(p => p.Price >= minPrice.Value);
			}

			if (maxPrice.HasValue)
			{
				query = query.Where(p => p.Price <= maxPrice.Value);
			}

			query = sortOrder switch
			{
				"price_asc" => query.OrderBy(p => p.Price),
				"price_desc" => query.OrderByDescending(p => p.Price),
				_ => query.OrderBy(p => p.Id)
			};

			viewModel.Parts = await query.ToListAsync();

			return View("Search", viewModel);
		}
	}
}