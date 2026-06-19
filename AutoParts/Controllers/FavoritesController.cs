using System.Security.Claims;
using AutoParts.Data;
using AutoParts.Models;
using AutoParts.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoParts.Controllers
{
	[Authorize]
	public class FavoritesController : Controller
	{
		private readonly ApplicationDbContext _context;

		public FavoritesController(ApplicationDbContext context)
		{
			_context = context;
		}

		[HttpGet]
		public async Task<IActionResult> Index()
		{
			int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

			var items = await _context.FavoriteItems
				.Include(f => f.AutoPart)
				.ThenInclude(p => p!.Category)
				.Where(f => f.UserId == userId)
				.ToListAsync();

			var viewModel = new FavoritesViewModel { Items = items };
			return View("~/Views/UserItems/Favorites.cshtml", viewModel);
		}

		[HttpPost]
		public async Task<IActionResult> ToggleFavorite(int partId)
		{
			var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userIdString)) return Challenge();

			int userId = int.Parse(userIdString);

			var favoriteItem = await _context.FavoriteItems
				.FirstOrDefaultAsync(f => f.UserId == userId && f.AutoPartId == partId);

			bool isAdded;

			if (favoriteItem == null)
			{
				_context.FavoriteItems.Add(new FavoriteItem
				{
					UserId = userId,
					AutoPartId = partId
				});
				isAdded = true;
			}
			else
			{
				_context.FavoriteItems.Remove(favoriteItem);
				isAdded = false;
			}

			await _context.SaveChangesAsync();

			var totalCount = await _context.FavoriteItems.CountAsync(f => f.UserId == userId);

			return Json(new { success = true, isAdded = isAdded, count = totalCount });
		}
	}
}