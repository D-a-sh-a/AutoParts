using AutoParts.Data;
using AutoParts.Entities;
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

		public ManagerController(ApplicationDbContext context, EmailService emailService, IWebHostEnvironment webHostEnvironment, ILogger<ManagerController> logger)
		{
			_context = context;
			_emailService = emailService;
			_webHostEnvironment = webHostEnvironment;
			_logger = logger;
		}

		// --- ДОПОМІЖНІ МЕТОДИ ---
		public static string GetStatusDisplayName(string status) => status switch
		{
			"Pending" => "В очікуванні",
			"Processing" => "В обробці",
			"Shipped" => "Відправлено",
			"Completed" => "Виконано",
			"Cancelled" => "Скасовано",
			_ => status
		};

		public static string GetCancelReasonDisplayName(CancelReason reason) => reason switch
		{
			CancelReason.ChangedMind => "Передумав",
			CancelReason.FoundCheaper => "Знайшов дешевше",
			CancelReason.DeliveryTooLong => "Довга доставка",
			CancelReason.OrderedByMistake => "Помилкове замовлення",
			CancelReason.OutOfStock => "Немає в наявності",
			CancelReason.Other => "Інше",
			_ => "Не обрано"
		};

		private async Task<List<string>> UploadImages(List<IFormFile>? files)
		{
			var urls = new List<string>();
			if (files == null || !files.Any()) return urls;
			string folder = Path.Combine(_webHostEnvironment.WebRootPath, "images/products");
			if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
			foreach (var f in files)
			{
				string name = Guid.NewGuid() + Path.GetExtension(f.FileName);
				using var fs = new FileStream(Path.Combine(folder, name), FileMode.Create);
				await f.CopyToAsync(fs);
				urls.Add("/images/products/" + name);
			}
			return urls;
		}

		// ==========================================
		// ЗАМОВЛЕННЯ
		// ==========================================
		[HttpGet]
		public async Task<IActionResult> Index(OrderStatus? statusFilter)
		{
			var query = _context.Orders.Include(o => o.Customer).AsQueryable();
			if (statusFilter.HasValue) query = query.Where(o => o.Status == statusFilter.Value);

			var rawOrders = await query.OrderByDescending(o => o.OrderDate).Take(20).ToListAsync();

			var viewModel = new ManagerDashboardViewModel
			{
				NewOrdersCount = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Pending),
				TotalPartsCount = await _context.AutoParts.CountAsync(),
				LowStockCount = await _context.AutoParts.CountAsync(p => p.StockQuantity <= 5),
				RecentOrders = rawOrders.Select(o => new ManagerDashboardOrderViewModel
				{
					Id = o.Id,
					CustomerName = o.Customer != null ? $"{o.Customer.FirstName} {o.Customer.LastName}" : "Невідомо",
					OrderDate = o.OrderDate,
					TotalSum = o.TotalAmount,
					Status = GetStatusDisplayName(o.Status.ToString()),
					TrackingNumber = o.TrackingNumber,
					CancelReason = o.CancelReason
				}).ToList()
			};
			return View("~/Views/Manager/Index.cshtml", viewModel);
		}

		[HttpGet]
		public async Task<IActionResult> EditOrder(int id)
		{
			var order = await _context.Orders
				.Include(o => o.OrderItems).ThenInclude(oi => oi.AutoPart)
				.Include(o => o.Customer)
				.FirstOrDefaultAsync(o => o.Id == id);

			if (order == null) return NotFound();

			ViewBag.AvailableParts = await _context.AutoParts.OrderBy(p => p.Name).ToListAsync();
			return View("~/Views/Manager/EditOrder.cshtml", order);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> EditOrder(Order model, string? customReason)
		{
			var order = await _context.Orders.Include(o => o.OrderItems).FirstOrDefaultAsync(o => o.Id == model.Id);
			if (order == null) return NotFound();

			if (order.Status == OrderStatus.Completed) return RedirectToAction("Index");

			if (order.Status == OrderStatus.Shipped)
			{
				if (model.Status == OrderStatus.Cancelled)
				{
					order.Status = OrderStatus.Cancelled;
					order.CancelReason = model.CancelReason;
					if (model.CancelReason == CancelReason.Other)
						order.Comment = !string.IsNullOrWhiteSpace(customReason) ? customReason : order.Comment;
				}
			}
			else
			{
				order.Status = model.Status;
				order.TrackingNumber = model.TrackingNumber;
				order.CancelReason = model.CancelReason;

				if (model.Status == OrderStatus.Cancelled && model.CancelReason == CancelReason.Other)
					order.Comment = !string.IsNullOrWhiteSpace(customReason) ? customReason : order.Comment;

				if (model.OrderItems != null)
				{
					foreach (var item in model.OrderItems)
					{
						var dbItem = order.OrderItems.FirstOrDefault(oi => oi.Id == item.Id);
						if (dbItem != null) dbItem.Quantity = item.Quantity;
					}
				}
				order.TotalAmount = order.OrderItems.Sum(i => i.Quantity * i.UnitPrice);
			}

			await _context.SaveChangesAsync();
			return RedirectToAction("Index");
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> AddOrderItem(int orderId, int partId, int quantity)
		{
			var order = await _context.Orders.Include(o => o.OrderItems).FirstOrDefaultAsync(o => o.Id == orderId);
			if (order == null || order.Status == OrderStatus.Completed || order.Status == OrderStatus.Shipped)
				return RedirectToAction("EditOrder", new { id = orderId });

			var part = await _context.AutoParts.FindAsync(partId);
			if (part == null) return NotFound();

			var existingItem = order.OrderItems.FirstOrDefault(oi => oi.AutoPartId == partId);
			if (existingItem != null)
			{
				existingItem.Quantity += quantity;
			}
			else
			{
				var newItem = new OrderItem
				{
					OrderId = orderId,
					AutoPartId = partId,
					Quantity = quantity,
					UnitPrice = part.Price
				};
				_context.OrderItems.Add(newItem);
				order.OrderItems.Add(newItem);
			}

			order.TotalAmount = order.OrderItems.Sum(i => i.Quantity * i.UnitPrice);
			await _context.SaveChangesAsync();
			return RedirectToAction("EditOrder", new { id = orderId });
		}

		[HttpPost]
		public async Task<IActionResult> RemoveOrderItem(int orderItemId)
		{
			var item = await _context.OrderItems.Include(oi => oi.Order).FirstOrDefaultAsync(oi => oi.Id == orderItemId);
			if (item != null)
			{
				var order = item.Order;
				_context.OrderItems.Remove(item);
				if (order != null) order.TotalAmount -= (item.Quantity * item.UnitPrice);
				await _context.SaveChangesAsync();
				return Json(new { success = true });
			}
			return Json(new { success = false });
		}

		// ==========================================
		// ІНВЕНТАРИЗАЦІЯ
		// ==========================================
		[HttpGet]
		public async Task<IActionResult> Inventory(string? searchTerm, bool lowStockOnly = false)
		{
			var query = _context.AutoParts.Include(p => p.Category).Include(p => p.Brand).AsQueryable();
			if (!string.IsNullOrEmpty(searchTerm)) query = query.Where(p => p.Name.Contains(searchTerm));
			if (lowStockOnly) query = query.Where(p => p.StockQuantity <= 5);

			return View("~/Views/Manager/Inventory.cshtml", await query.OrderBy(p => p.Name).ToListAsync());
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> UpdateAllStock(Dictionary<int, int> stocks, Dictionary<int, decimal> prices)
		{
			foreach (var entry in stocks)
			{
				var part = await _context.AutoParts.FindAsync(entry.Key);
				if (part != null)
				{
					part.StockQuantity = entry.Value;
					if (prices.TryGetValue(entry.Key, out decimal newPrice) && newPrice > 0)
					{
						part.Price = newPrice;
					}
				}
			}
			await _context.SaveChangesAsync();
			return Json(new { success = true, message = "Залишки та ціни успішно оновлено!" });
		}

		// ==========================================
		// ТОВАРИ (СТВОРЕННЯ ТА РЕДАГУВАННЯ)
		// ==========================================

		// Метод для заповнення ViewBag (щоб не дублювати код)
		private async Task LoadDropdownDataAsync()
		{
			ViewBag.Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
			ViewBag.Brands = await _context.Brands.OrderBy(b => b.Name).ToListAsync();
			ViewBag.Vehicles = await _context.Vehicles
				.Select(v => new { Id = v.Id, MakeName = v.Make.Name, ModelName = v.Model.Name, Year = v.Year })
				.OrderBy(v => v.MakeName).ThenBy(v => v.ModelName)
				.ToListAsync();
		}

		[HttpGet]
		public async Task<IActionResult> CreatePart()
		{
			await LoadDropdownDataAsync();
			return View("~/Views/Manager/PartForm.cshtml", new ProductFormViewModel());
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CreatePart(ProductFormViewModel model)
		{
			if (!ModelState.IsValid)
			{
				await LoadDropdownDataAsync();
				return View("~/Views/Manager/PartForm.cshtml", model);
			}

			var autoPart = new AutoPart
			{
				SKU = model.SKU,
				Name = model.Name,
				Price = model.Price,
				StockQuantity = model.StockQuantity,
				Description = model.Description ?? "",
				CategoryId = model.CategoryId,
				BrandId = model.BrandId,
				ImageUrls = await UploadImages(model.ImageFiles)
			};

			if (model.SelectedVehicleIds != null && model.SelectedVehicleIds.Any())
			{
				autoPart.Vehicles = await _context.Vehicles
					.Where(v => model.SelectedVehicleIds.Contains(v.Id))
					.ToListAsync();
			}

			_context.AutoParts.Add(autoPart);
			await _context.SaveChangesAsync();
			return RedirectToAction("Inventory");
		}

		[HttpGet]
		public async Task<IActionResult> EditPart(int id)
		{
			var part = await _context.AutoParts
				.Include(p => p.Vehicles)
				.FirstOrDefaultAsync(p => p.Id == id);

			if (part == null) return NotFound();

			await LoadDropdownDataAsync();

			var model = new ProductFormViewModel
			{
				Id = part.Id,
				SKU = part.SKU,
				Name = part.Name,
				Price = part.Price,
				StockQuantity = part.StockQuantity,
				Description = part.Description,
				CategoryId = part.CategoryId,
				BrandId = part.BrandId ?? 0,
				ExistingImageUrls = part.ImageUrls,
				SelectedVehicleIds = part.Vehicles?.Select(v => v.Id).ToList() ?? new List<int>()
			};

			return View("~/Views/Manager/PartForm.cshtml", model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> EditPart(ProductFormViewModel model)
		{
			if (!ModelState.IsValid)
			{
				await LoadDropdownDataAsync();
				return View("~/Views/Manager/PartForm.cshtml", model);
			}

			var part = await _context.AutoParts
				.Include(p => p.Vehicles)
				.FirstOrDefaultAsync(p => p.Id == model.Id);

			if (part == null) return NotFound();

			part.SKU = model.SKU;
			part.Name = model.Name;
			part.Price = model.Price;
			part.StockQuantity = model.StockQuantity;
			part.Description = model.Description ?? "";
			part.CategoryId = model.CategoryId;
			part.BrandId = model.BrandId;

			if (model.ImageFiles != null && model.ImageFiles.Any())
			{
				part.ImageUrls = await UploadImages(model.ImageFiles);
			}

			if (part.Vehicles != null)
			{
				part.Vehicles.Clear();
			}
			else
			{
				part.Vehicles = new List<Vehicle>();
			}

			if (model.SelectedVehicleIds != null && model.SelectedVehicleIds.Any())
			{
				var selectedVehicles = await _context.Vehicles
					.Where(v => model.SelectedVehicleIds.Contains(v.Id))
					.ToListAsync();

				foreach (var vehicle in selectedVehicles)
				{
					part.Vehicles.Add(vehicle);
				}
			}

			await _context.SaveChangesAsync();
			return RedirectToAction("Inventory");
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CreateCategory(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				return Json(new { success = false, message = "Назва категорії не може бути порожньою." });
			}

			// Перевіряємо, чи немає вже такої категорії, щоб уникнути дублів
			var existing = await _context.Categories.FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());
			if (existing != null)
			{
				return Json(new { success = false, message = "Така категорія вже існує." });
			}

			var category = new Category { Name = name };
			_context.Categories.Add(category);
			await _context.SaveChangesAsync();

			// Повертаємо JSON для нашого JavaScript (manager.js)
			return Json(new { success = true, id = category.Id, name = category.Name });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CreateBrand(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				return Json(new { success = false, message = "Назва бренду не може бути порожньою." });
			}

			var existing = await _context.Brands.FirstOrDefaultAsync(b => b.Name.ToLower() == name.ToLower());
			if (existing != null)
			{
				return Json(new { success = false, message = "Такий бренд вже існує." });
			}

			var brand = new Brand { Name = name };
			_context.Brands.Add(brand);
			await _context.SaveChangesAsync();

			return Json(new { success = true, id = brand.Id, name = brand.Name });
		}
	}
}