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

			if (favoriteItem == null)
			{
				_context.FavoriteItems.Add(new FavoriteItem
				{
					UserId = userId,
					AutoPartId = partId
				});
			}
			else
			{
				_context.FavoriteItems.Remove(favoriteItem);
			}

			await _context.SaveChangesAsync();
			return RedirectRequest();
		}

		private IActionResult RedirectRequest()
		{
			var referer = Request.Headers["Referer"].ToString();
			if (!string.IsNullOrEmpty(referer)) return Redirect(referer);
			return RedirectToAction("Index", "Home");
		}
	}
}