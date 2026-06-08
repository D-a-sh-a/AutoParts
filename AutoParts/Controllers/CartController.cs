using AutoParts.Data;
using AutoParts.Models;
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

		private IActionResult RedirectRequest()
		{
			var referer = Request.Headers["Referer"].ToString();
			if (!string.IsNullOrEmpty(referer)) return Redirect(referer);
			return RedirectToAction("Index", "Home");
		}
	}
}