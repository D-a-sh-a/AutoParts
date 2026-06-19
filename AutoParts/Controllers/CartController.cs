using AutoParts.Data;
using AutoParts.Models;
using AutoParts.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AutoParts.Controllers
{
	public class CartController : Controller
	{
		private readonly ApplicationDbContext _context;

		public CartController(ApplicationDbContext context)
		{
			_context = context;
		}

		[HttpGet]
		public async Task<IActionResult> Index()
		{
			var cartId = GetCartId();

			var items = await _context.CartItems
				.Include(c => c.AutoPart)
				.Where(c => c.CartId == cartId)
				.ToListAsync();

			var viewModel = new CartViewModel
			{
				Items = items
			};

			return View("~/Views/UserItems/Cart.cshtml", viewModel);
		}

		[HttpPost]
		public async Task<IActionResult> AddToCart(int partId, int quantity = 1)
		{
			var cartId = GetCartId();

			var cartItem = await _context.CartItems
				.FirstOrDefaultAsync(c => c.CartId == cartId && c.AutoPartId == partId);

			if (cartItem == null)
			{
				cartItem = new CartItem
				{
					CartId = cartId,
					AutoPartId = partId,
					Quantity = quantity
				};
				_context.CartItems.Add(cartItem);
			}
			else
			{
				cartItem.Quantity += quantity;
			}

			await _context.SaveChangesAsync();

			var totalItemsCount = await _context.CartItems
				.Where(c => c.CartId == cartId)
				.SumAsync(c => c.Quantity);

			return Json(new { success = true, count = totalItemsCount });
		}

		[HttpGet]
		public async Task<IActionResult> GetCartAndFavoritesCount()
		{
			var cartId = GetCartId();

			var cartCount = await _context.CartItems
				.Where(c => c.CartId == cartId)
				.SumAsync(c => c.Quantity);

			var favoritesCount = 0;
			var favoriteIds = new List<int>();

			if (User.Identity != null && User.Identity.IsAuthenticated)
			{
				var userIdString = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
				if (!string.IsNullOrEmpty(userIdString))
				{
					int userId = int.Parse(userIdString);

					var favItems = await _context.FavoriteItems
						.Where(f => f.UserId == userId)
						.ToListAsync();

					favoritesCount = favItems.Count;
					favoriteIds = favItems.Select(f => f.AutoPartId).ToList();
				}
			}

			return Json(new
			{
				cartCount = cartCount,
				favoritesCount = favoritesCount,
				favoriteIds = favoriteIds
			});
		}

		[HttpPost]
		public async Task<IActionResult> RemoveFromCart(int id)
		{
			var cartId = GetCartId();

			var cartItem = await _context.CartItems
				.FirstOrDefaultAsync(c => c.Id == id && c.CartId == cartId);

			if (cartItem != null)
			{
				_context.CartItems.Remove(cartItem);
				await _context.SaveChangesAsync();
			}

			return RedirectToAction("Index");
		}

		[HttpPost]
		public async Task<IActionResult> UpdateQuantity(int id, int quantity)
		{
			if (quantity < 1) return RedirectToAction("Index");

			var cartId = GetCartId();
			var cartItem = await _context.CartItems
				.FirstOrDefaultAsync(c => c.Id == id && c.CartId == cartId);

			if (cartItem != null)
			{
				cartItem.Quantity = quantity;
				await _context.SaveChangesAsync();
			}

			return RedirectToAction("Index");
		}

		private string GetCartId()
		{
			var cartId = HttpContext.Session.GetString("CartId");
			if (string.IsNullOrEmpty(cartId))
			{
				cartId = Guid.NewGuid().ToString();
				HttpContext.Session.SetString("CartId", cartId);
			}
			return cartId;
		}
	}
}