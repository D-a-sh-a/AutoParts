using AutoParts.Data;
using AutoParts.Models;
using AutoParts.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AutoParts.Controllers
{
	public class OrderController : Controller
	{
		private readonly ApplicationDbContext _context;

		public OrderController(ApplicationDbContext context)
		{
			_context = context;
		}

		[HttpGet]
		public async Task<IActionResult> Checkout()
		{
			var cartId = HttpContext.Session.GetString("CartId");
			if (string.IsNullOrEmpty(cartId)) return RedirectToAction("Index", "Cart");

			var cartItems = await _context.CartItems
				.Include(c => c.AutoPart)
				.Where(c => c.CartId == cartId)
				.ToListAsync();

			if (!cartItems.Any()) return RedirectToAction("Index", "Cart");

			var viewModel = new CheckoutViewModel
			{
				CartItems = cartItems,
				TotalAmount = cartItems.Sum(i => i.Quantity * i.AutoPart!.Price)
			};

			return View("~/Views/Order/Checkout.cshtml", viewModel);
		}

		[HttpPost]
		public async Task<IActionResult> Checkout(CheckoutViewModel model)
		{
			var cartId = HttpContext.Session.GetString("CartId");
			var cartItems = await _context.CartItems
				.Include(c => c.AutoPart)
				.Where(c => c.CartId == cartId)
				.ToListAsync();

			if (!cartItems.Any()) return RedirectToAction("Index", "Cart");

			if (!ModelState.IsValid)
			{
				model.CartItems = cartItems;
				model.TotalAmount = cartItems.Sum(i => i.Quantity * i.AutoPart!.Price);
				return View("~/Views/Order/Checkout.cshtml", model);
			}

			int? currentUserId = null;
			if (User.Identity != null && User.Identity.IsAuthenticated)
			{
				var userIdClaim = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
				if (!string.IsNullOrEmpty(userIdClaim))
				{
					currentUserId = int.Parse(userIdClaim);
				}
			}

			var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == model.Email);

			if (customer == null)
			{
				customer = new Customer
				{
					FirstName = model.FirstName,
					LastName = model.LastName,
					Email = model.Email,
					Phone = model.Phone,
					UserId = currentUserId
				};
				_context.Customers.Add(customer);
			}
			else
			{
				customer.FirstName = model.FirstName;
				customer.LastName = model.LastName;
				customer.Phone = model.Phone;

				if (customer.UserId == null && currentUserId != null)
				{
					customer.UserId = currentUserId;
				}

				_context.Customers.Update(customer);
			}

			await _context.SaveChangesAsync();

			var order = new Order
			{
				CustomerId = customer.Id,
				OrderDate = DateTime.UtcNow,
				TotalAmount = cartItems.Sum(item => item.Quantity * item.AutoPart!.Price),
				ShippingAddress = model.Address,
				Comment = model.Comment,
				Status = AutoParts.Enums.OrderStatus.Pending
			};

			_context.Orders.Add(order);
			await _context.SaveChangesAsync();

			foreach (var item in cartItems)
			{
				var orderItem = new OrderItem
				{
					OrderId = order.Id,
					AutoPartId = item.AutoPartId,
					Quantity = item.Quantity,
					UnitPrice = item.AutoPart!.Price
				};
				_context.OrderItems.Add(orderItem);

				if (item.AutoPart != null && item.AutoPart.StockQuantity >= item.Quantity)
				{
					item.AutoPart.StockQuantity -= item.Quantity;
				}
			}

			_context.CartItems.RemoveRange(cartItems);
			await _context.SaveChangesAsync();

			HttpContext.Session.Remove("CartId");

			return RedirectToAction("Success", new { orderId = order.Id });
		}

		[HttpGet]
		public IActionResult Success(int orderId)
		{
			ViewBag.OrderId = orderId;
			return View("~/Views/Order/Success.cshtml");
		}
	}
}