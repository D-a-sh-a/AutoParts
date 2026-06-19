using AutoParts.Data;
using AutoParts.Models;
using AutoParts.Services;
using AutoParts.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AutoParts.Controllers
{
	public class OrderController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly EmailService _emailService;

		public OrderController(ApplicationDbContext context, EmailService emailService)
		{
			_context = context;
			_emailService = emailService;
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
		[ValidateAntiForgeryToken]
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

			Customer? customer = null;

			if (currentUserId != null)
			{
				customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == currentUserId);
			}

			if (customer == null)
			{
				customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == model.Email);
			}

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

			string emailItemsRows = "";

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

				emailItemsRows += $@"
					<tr style='border-bottom: 1px solid #eee;'>
						<td style='padding: 10px 0; color: #333;'>{item.AutoPart.Name}</td>
						<td style='padding: 10px 0; text-align: center; color: #666;'>{item.Quantity} шт.</td>
						<td style='padding: 10px 0; text-align: right; font-weight: bold; color: #2b2d42;'>{(item.Quantity * item.AutoPart.Price).ToString("0")} грн</td>
					</tr>";

				if (item.AutoPart != null && item.AutoPart.StockQuantity >= item.Quantity)
				{
					item.AutoPart.StockQuantity -= item.Quantity;
				}
			}

			_context.CartItems.RemoveRange(cartItems);
			await _context.SaveChangesAsync();

			string subject = $"Замовлення №{order.Id} успешно оформлено! - AUTOPARTS";
			string body = $@"
				<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e1e1e1; border-radius: 12px;'>
					<div style='text-align: center; border-bottom: 2px solid #ef233c; padding-bottom: 15px;'>
						<h2 style='color: #ef233c; margin: 0; font-size: 28px;'>AUTO<span style='color: #2b2d42;'>PARTS</span></h2>
						<p style='color: #777; margin: 5px 0 0 0;'>Дякуємо за покупку, {model.FirstName}!</p>
					</div>
					
					<div style='margin: 20px 0;'>
						<h4 style='color: #2b2d42; margin-bottom: 10px; border-bottom: 1px solid #ddd; padding-bottom: 5px;'>📋 Деталі замовлення №{order.Id}</h4>
						<table style='width: 100%; border-collapse: collapse; font-size: 14px;'>
							<thead>
								<tr style='background-color: #f8f9fa; color: #777; text-align: left;'>
									<th style='padding: 8px 0;'>Товар</th>
									<th style='padding: 8px 0; text-align: center;'>К-сть</th>
									<th style='padding: 8px 0; text-align: right;'>Сума</th>
								</tr>
							</thead>
							<tbody>
								{emailItemsRows}
							</tbody>
						</table>
					</div>

					<div style='background-color: #f8f9fa; padding: 15px; border-radius: 8px; margin-top: 20px;'>
						<p style='margin: 0 0 8px 0; font-size: 14px;'><strong>📍 Адреса доставки:</strong> {model.Address}</p>
						<p style='margin: 0 0 8px 0; font-size: 14px;'><strong>📞 Телефон:</strong> {model.Phone}</p>
						{(!string.IsNullOrEmpty(model.Comment) ? $"<p style='margin: 0; font-size: 14px;'><strong>💬 Коментар:</strong> <i>«{model.Comment}»</i></p>" : "")}
					</div>

					<div style='text-align: right; margin-top: 25px; padding-top: 15px; border-top: 2px dashed #ddd;'>
						<span style='font-size: 16px; color: #555; font-weight: bold;'>Загальна вартість:</span>
						<span style='font-size: 22px; color: #ef233c; font-weight: bold; margin-left: 10px;'>{order.TotalAmount.ToString("0")} грн</span>
					</div>

					<div style='text-align: center; margin-top: 30px; font-size: 12px; color: #999; border-top: 1px solid #eee; padding-top: 15px;'>
						Наш менеджер зв'яжеться з вами найближчим часом для підтвердження відправки.<br/>
						© {DateTime.Now.Year} AUTOPARTS Store. Всі права захищені.
					</div>
				</div>";

			try
			{
				await _emailService.SendEmailAsync(model.Email, subject, body);
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Не вдалося надіслати чек на пошту: {ex.Message}");
			}

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