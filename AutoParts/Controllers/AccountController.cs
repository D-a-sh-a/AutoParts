using AutoParts.Entities;
using AutoParts.Enums;
using AutoParts.Models;
using AutoParts.Services;
using AutoParts.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AutoParts.Controllers
{
	public class AccountController : Controller
	{
		private readonly UserManager<DbUser> _userManager;
		private readonly SignInManager<DbUser> _signInManager;
		private readonly AutoParts.Data.ApplicationDbContext _context;
		private readonly EmailService _emailService;

		public AccountController(
			UserManager<DbUser> userManager,
			SignInManager<DbUser> signInManager,
			AutoParts.Data.ApplicationDbContext context,
			EmailService emailService)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_context = context;
			_emailService = emailService;
		}

		[HttpGet]
		public IActionResult Login()
		{
			if (User.Identity != null && User.Identity.IsAuthenticated)
			{
				return RedirectToAction("Index", "Home");
			}
			return View("~/Views/Account/Login.cshtml", new LoginViewModel());
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
		{
			if (!ModelState.IsValid) return View("~/Views/Account/Login.cshtml", model);

			var user = await _userManager.FindByEmailAsync(model.Email);
			if (user != null)
			{
				if (!await _userManager.IsEmailConfirmedAsync(user))
				{
					ModelState.AddModelError(string.Empty, "Ви не підтвердили свою електронну пошту. Будь ласка, перевірте скриньку.");
					return View("~/Views/Account/Login.cshtml", model);
				}

				var result = await _signInManager.PasswordSignInAsync(user.UserName!, model.Password, model.RememberMe, lockoutOnFailure: false);
				if (result.Succeeded)
				{
					if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
					{
						return Redirect(returnUrl);
					}

					if (await _userManager.IsInRoleAsync(user, "Manager"))
					{
						return RedirectToAction("Index", "Manager");
					}

					return RedirectToAction("Index", "Account");
				}
			}

			ModelState.AddModelError(string.Empty, "Невірний логін або пароль.");
			return View("~/Views/Account/Login.cshtml", model);
		}

		[HttpGet]
		public IActionResult Register()
		{
			if (User.Identity != null && User.Identity.IsAuthenticated)
			{
				return RedirectToAction("Index", "Home");
			}
			return View("~/Views/Account/Register.cshtml", new RegisterViewModel());
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Register(RegisterViewModel model)
		{
			if (!ModelState.IsValid) return View("~/Views/Account/Register.cshtml", model);

			var user = new DbUser
			{
				UserName = model.Email,
				Email = model.Email,
				PhoneNumber = model.Phone,
				FirstName = model.FirstName,
				LastName = model.LastName
			};

			var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Client");

                var existingCustomer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == model.Email);

                if (existingCustomer != null)
                {
                    existingCustomer.UserId = user.Id;

                    existingCustomer.FirstName = model.FirstName;
                    existingCustomer.LastName = model.LastName;
                    existingCustomer.Phone = model.Phone;

                    _context.Customers.Update(existingCustomer);
                }
                else
                {
                    var customer = new Customer
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Email = model.Email,
                        Phone = model.Phone,
                        UserId = user.Id
                    };
                    _context.Customers.Add(customer);
                }

                await _context.SaveChangesAsync();

                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
				var confirmationLink = Url.Action("ConfirmEmail", "Account",
					new { userId = user.Id, token = token },
					Request.Scheme);
				string subject = "Підтвердження реєстрації - AUTOPARTS";
				string body = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #eee; border-radius: 8px;'>
                        <h2 style='color: #ef233c; text-align: center;'>AUTO<span style='color: #2b2d42;'>PARTS</span></h2>
                        <p>Вітаємо, {model.FirstName}!</p>
                        <p>Дякуємо за реєстрацію в нашому магазині автозапчастин. Для активації вашого акаунта, будь ласка, натисніть на кнопку нижче:</p>
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{confirmationLink}' style='background-color: #ef233c; color: white; padding: 12px 25px; text-decoration: none; font-weight: bold; border-radius: 5px; display: inline-block;'>ПІДТВЕРДИТИ АКАУНТ</a>
                        </div>
                        <p style='font-size: 0.8rem; color: #777;'>Якщо ви не реєструвалися на нашому сайті, просто проігноруйте цей лист.</p>
                    </div>";

				try
				{
					await _emailService.SendEmailAsync(model.Email, subject, body);
				}
				catch (Exception ex)
				{
					System.Diagnostics.Debug.WriteLine($"Помилка відправки листа: {ex.Message}");
				}

				return RedirectToAction("RegisterSuccess", new { email = model.Email });
			}

			foreach (var error in result.Errors)
			{
				ModelState.AddModelError(string.Empty, error.Description);
			}

			return View("~/Views/Account/Register.cshtml", model);
		}

		[HttpGet]
		public IActionResult RegisterSuccess(string email)
		{
			if (string.IsNullOrEmpty(email)) return RedirectToAction("Index", "Home");
			ViewBag.Email = email;
			return View("~/Views/Account/RegisterSuccess.cshtml");
		}

		[HttpGet]
		public async Task<IActionResult> ConfirmEmail(int userId, string token)
		{
			if (userId <= 0 || string.IsNullOrEmpty(token))
			{
				return RedirectToAction("Index", "Home");
			}

			var user = await _userManager.FindByIdAsync(userId.ToString());
			if (user == null)
			{
				return NotFound($"Користувача з ID {userId} не знайдено.");
			}

			var result = await _userManager.ConfirmEmailAsync(user, token);
			if (result.Succeeded)
			{
				return View("~/Views/Account/ConfirmEmailSuccess.cshtml");
			}

			ModelState.AddModelError("", "Посилання застаріло або є недійсним.");
			return View("Error");
		}

		[Authorize]
		[HttpGet]
		public async Task<IActionResult> Index()
		{
			int userId = int.Parse(_userManager.GetUserId(User)!);

			var customerData = await _context.Customers
				.Include(c => c.Orders)
					.ThenInclude(o => o.OrderItems)
						.ThenInclude(oi => oi.AutoPart)
				.FirstOrDefaultAsync(c => c.UserId == userId);

			if (customerData == null) return RedirectToAction("Index", "Home");

			return View("~/Views/Account/Index.cshtml", customerData);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize]
		public async Task<IActionResult> CancelOrder([FromForm] CancelOrderViewModel model)
		{
			int userId = int.Parse(_userManager.GetUserId(User)!);

			var order = await _context.Orders
				.Include(o => o.Customer)
				.FirstOrDefaultAsync(o => o.Id == model.OrderId && o.Customer!.UserId == userId);

			if (order == null)
				return Json(new { success = false, message = "Замовлення не знайдено або у вас немає до нього доступу." });

			if (order.Status != OrderStatus.Pending)
				return Json(new { success = false, message = "Можна скасувати тільки нові замовлення." });

			order.Status = OrderStatus.Cancelled;
			order.CancelReason = model.Reason;

			if (model.Reason == CancelReason.Other && !string.IsNullOrWhiteSpace(model.CustomReason))
			{
				string cancelText = $"Причина скасування: {model.CustomReason}";

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

			return Json(new { success = true, message = "Замовлення успішно скасовано." });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Logout()
		{
			await _signInManager.SignOutAsync();
			return RedirectToAction("Index", "Home");
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize]
		public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword, string confirmPassword)
		{
			if (newPassword != confirmPassword)
			{
				return Json(new { success = false, message = "Новий пароль та підтвердження не збігаються." });
			}

			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				return Json(new { success = false, message = "Помилка авторизації. Користувача не знайдено." });
			}

			var result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);

			if (result.Succeeded)
			{
				await _signInManager.RefreshSignInAsync(user);

				return Json(new { success = true, message = "Пароль успішно оновлено!" });
			}
			else
			{
				var errorMsg = string.Join("<br/>", result.Errors.Select(e => e.Description));
				return Json(new { success = false, message = errorMsg });
			}
		}
	}
}