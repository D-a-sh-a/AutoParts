using AutoParts.Data;
using AutoParts.Enums;
using AutoParts.Models;
using AutoParts.Services;
using AutoParts.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AutoParts.Controllers
{
	[Authorize(Roles = "Manager")]
	public class ManagerController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly EmailService _emailService;
		private readonly IWebHostEnvironment _webHostEnvironment;
		private readonly ILogger<ManagerController> _logger;

		public ManagerController(
			ApplicationDbContext context,
			EmailService emailService,
			IWebHostEnvironment webHostEnvironment,
			ILogger<ManagerController> logger)
		{
			_context = context;
			_emailService = emailService;
			_webHostEnvironment = webHostEnvironment;
			_logger = logger;
		}

		[HttpGet]
		public async Task<IActionResult> Index()
		{
			var newOrdersCount = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Pending);
			var totalPartsCount = await _context.AutoParts.CountAsync();
			var lowStockCount = await _context.AutoParts.CountAsync(p => p.StockQuantity <= 5);

			var rawOrders = await _context.Orders
				.Include(o => o.Customer)
				.OrderByDescending(o => o.OrderDate)
				.Take(10)
				.ToListAsync();

			var recentOrders = rawOrders.Select(o => new ManagerDashboardOrderViewModel
			{
				Id = o.Id,
				CustomerName = o.Customer != null ? $"{o.Customer.FirstName} {o.Customer.LastName}" : "Невідомо",
				OrderDate = o.OrderDate,
				TotalSum = o.TotalAmount,
				Status = GetOrderStatusName(o.Status)
			}).ToList();

			var viewModel = new ManagerDashboardViewModel
			{
				NewOrdersCount = newOrdersCount,
				TotalPartsCount = totalPartsCount,
				LowStockCount = lowStockCount,
				RecentOrders = recentOrders
			};

			return View("~/Views/Manager/Index.cshtml", viewModel);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ShipOrder([FromForm] ShipOrderViewModel model)
		{
			var order = await _context.Orders
				.Include(o => o.Customer)
				.FirstOrDefaultAsync(o => o.Id == model.OrderId);

			if (order == null) return Json(new { success = false, message = "Замовлення не знайдено." });

			order.Status = OrderStatus.Shipped;
			order.TrackingNumber = model.TrackingNumber;

			await _context.SaveChangesAsync();

			if (order.Customer != null && !string.IsNullOrEmpty(order.Customer.Email))
			{
				string subject = $"Ваше замовлення #{order.Id} відправлено!";
				string body = $@"
                    <h2 style='color: #ef233c;'>AUTO<span style='color: #2b2d42;'>PARTS</span></h2>
                    <p>Вітаємо, {order.Customer.FirstName}!</p>
                    <p>Ваше замовлення <b>#{order.Id}</b> було успішно передано в службу доставки.</p>
                    <p><b>Номер накладної (ТТН):</b> <span style='font-size: 1.2rem; background: #eee; padding: 5px; font-weight: bold;'>{model.TrackingNumber}</span></p>
                    <p>Ви можете відстежувати його у своєму кабінеті на сайті.</p>";

				try
				{
					await _emailService.SendEmailAsync(order.Customer.Email, subject, body);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, $"Помилка відправки листа з ТТН для замовлення #{order.Id} на пошту {order.Customer.Email}");
				}
			}

			return Json(new { success = true, message = "Статус оновлено, ТТН збережено, лист надіслано!" });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CancelOrderManager([FromForm] CancelOrderViewModel model)
		{
			var order = await _context.Orders.FindAsync(model.OrderId);
			if (order == null) return Json(new { success = false, message = "Замовлення не знайдено." });

			order.Status = OrderStatus.Cancelled;
			order.CancelReason = model.Reason;

			if (model.Reason == CancelReason.Other && !string.IsNullOrWhiteSpace(model.CustomReason))
			{
				string cancelText = $"Причина скасування (Менеджер): {model.CustomReason}";

				if (string.IsNullOrWhiteSpace(order.Comment))
				{
					order.Comment = cancelText;
				}
				else
				{
					order.Comment += $"\n\n{cancelText}";
				}
			}

			await _context.SaveChangesAsync();
			return Json(new { success = true, message = "Замовлення скасовано менеджером." });
		}

		[HttpGet]
		public async Task<IActionResult> Inventory(string? searchTerm, bool lowStockOnly = false)
		{
			var query = _context.AutoParts
				.Include(p => p.Category)
				.Include(p => p.Brand)
				.AsQueryable();

			if (!string.IsNullOrEmpty(searchTerm))
			{
				query = query.Where(p => p.Name.Contains(searchTerm));
			}

			if (lowStockOnly)
			{
				query = query.Where(p => p.StockQuantity <= 5);
			}

			var items = await query.OrderBy(p => p.Name).ToListAsync();
			return View("~/Views/Manager/Inventory.cshtml", items);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> UpdateStock([FromForm] UpdateStockViewModel model)
		{
			if (model.NewQuantity < 0)
			{
				return Json(new { success = false, message = "Кількість не може бути від'ємною." });
			}

			var part = await _context.AutoParts.FindAsync(model.PartId);
			if (part == null)
			{
				return Json(new { success = false, message = "Запчастину не знайдено." });
			}

			part.StockQuantity = model.NewQuantity;
			await _context.SaveChangesAsync();

			return Json(new { success = true, message = $"Кількість '{part.Name}' змінено на {part.StockQuantity} шт." });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CreateCategory(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
				return Json(new { success = false, message = "Назва не може бути порожньою." });

			var exists = await _context.Categories.AnyAsync(c => c.Name.ToLower() == name.Trim().ToLower());
			if (exists)
				return Json(new { success = false, message = "Категорія вже існує." });

			var category = new Category { Name = name.Trim() };
			_context.Categories.Add(category);
			await _context.SaveChangesAsync();

			return Json(new { success = true, message = $"Категорію '{category.Name}' створено!" });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CreateBrand(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
				return Json(new { success = false, message = "Назва не може бути порожньою." });

			var exists = await _context.Brands.AnyAsync(b => b.Name.ToLower() == name.Trim().ToLower());
			if (exists)
				return Json(new { success = false, message = "Бренд вже існує." });

			var brand = new Brand { Name = name.Trim() };
			_context.Brands.Add(brand);
			await _context.SaveChangesAsync();

			return Json(new { success = true, message = $"Бренд '{brand.Name}' створено!" });
		}

		[HttpGet]
		public async Task<IActionResult> CreatePart()
		{
			ViewBag.Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
			ViewBag.Brands = await _context.Brands.OrderBy(b => b.Name).ToListAsync();
			return View("~/Views/Manager/PartForm.cshtml", new ProductFormViewModel());
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CreatePart(ProductFormViewModel model)
		{
			if (!ModelState.IsValid)
			{
				ViewBag.Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
				ViewBag.Brands = await _context.Brands.OrderBy(b => b.Name).ToListAsync();
				return View("~/Views/Manager/PartForm.cshtml", model);
			}

			List<string> imageUrls = await UploadImages(model.ImageFiles);

			var autoPart = new AutoPart
			{
				Name = model.Name,
				Price = model.Price,
				StockQuantity = model.StockQuantity,
				Description = model.Description,
				CategoryId = model.CategoryId,
				BrandId = model.BrandId,
				ImageUrls = imageUrls
			};

			_context.AutoParts.Add(autoPart);
			await _context.SaveChangesAsync();

			return RedirectToAction("Inventory");
		}

		[HttpGet]
		public async Task<IActionResult> EditPart(int id)
		{
			var part = await _context.AutoParts.FindAsync(id);
			if (part == null) return NotFound();

			var model = new ProductFormViewModel
			{
				Id = part.Id,
				Name = part.Name,
				Price = part.Price,
				StockQuantity = part.StockQuantity,
				Description = part.Description,
				CategoryId = part.CategoryId,
				BrandId = part.BrandId ?? 0,
				ExistingImageUrls = part.ImageUrls
			};

			ViewBag.Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
			ViewBag.Brands = await _context.Brands.OrderBy(b => b.Name).ToListAsync();

			return View("~/Views/Manager/PartForm.cshtml", model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> EditPart(ProductFormViewModel model)
		{
			if (!ModelState.IsValid)
			{
				ViewBag.Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
				ViewBag.Brands = await _context.Brands.OrderBy(b => b.Name).ToListAsync();
				return View("~/Views/Manager/PartForm.cshtml", model);
			}

			var part = await _context.AutoParts.FindAsync(model.Id);
			if (part == null) return NotFound();

			part.Name = model.Name;
			part.Price = model.Price;
			part.StockQuantity = model.StockQuantity;
			part.Description = model.Description;
			part.CategoryId = model.CategoryId;
			part.BrandId = model.BrandId;

			if (model.ImageFiles != null && model.ImageFiles.Any())
			{
				List<string> newUrls = await UploadImages(model.ImageFiles);
				part.ImageUrls ??= new List<string>();
				part.ImageUrls.AddRange(newUrls);
			}

			_context.AutoParts.Update(part);
			await _context.SaveChangesAsync();

			return RedirectToAction("Inventory");
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeletePart(int id)
		{
			var part = await _context.AutoParts.FindAsync(id);
			if (part == null) return Json(new { success = false, message = "Товар не знайдено." });

			_context.AutoParts.Remove(part);
			await _context.SaveChangesAsync();

			return Json(new { success = true, message = "Товар успішно видалено." });
		}

		private string GetOrderStatusName(OrderStatus status)
		{
			return status switch
			{
				OrderStatus.Pending => "В очікуванні",
				OrderStatus.Processing => "В обробці",
				OrderStatus.Shipped => "Відправлено",
				OrderStatus.Completed => "Виконано",
				OrderStatus.Cancelled => "Скасовано",
				_ => "Невідомо"
			};
		}

		private async Task<List<string>> UploadImages(List<IFormFile>? files)
		{
			var uploadedUrls = new List<string>();
			if (files == null || !files.Any()) return uploadedUrls;

			string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");

			if (!Directory.Exists(uploadsFolder))
			{
				Directory.CreateDirectory(uploadsFolder);
			}

			foreach (var file in files)
			{
				if (file.Length > 0)
				{
					string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
					string filePath = Path.Combine(uploadsFolder, uniqueFileName);

					using (var fileStream = new FileStream(filePath, FileMode.Create))
					{
						await file.CopyToAsync(fileStream);
					}

					uploadedUrls.Add("/images/products/" + uniqueFileName);
				}
			}

			return uploadedUrls;
		}
	}
}