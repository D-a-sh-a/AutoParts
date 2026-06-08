using AutoParts.Data;
using AutoParts.Models;
using AutoParts.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
			return RedirectRequest();
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

		private IActionResult RedirectRequest()
		{
			var referer = Request.Headers["Referer"].ToString();
			if (!string.IsNullOrEmpty(referer)) return Redirect(referer);
			return RedirectToAction("Index", "Home");
		}
	}
}