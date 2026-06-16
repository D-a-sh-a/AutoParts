using AutoParts.Entities;
using AutoParts.Models;
using AutoParts.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AutoParts.Controllers
{
	public class AccountController : Controller
	{
		private readonly UserManager<DbUser> _userManager;
		private readonly SignInManager<DbUser> _signInManager;
		private readonly AutoParts.Data.ApplicationDbContext _context;

		public AccountController(
			UserManager<DbUser> userManager,
			SignInManager<DbUser> signInManager,
			AutoParts.Data.ApplicationDbContext context)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_context = context;
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
				var result = await _signInManager.PasswordSignInAsync(user.UserName!, model.Password, model.RememberMe, lockoutOnFailure: false);
				if (result.Succeeded)
				{
					if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
					{
						return Redirect(returnUrl);
					}
					return RedirectToAction("Index", "Home");
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
				var customer = new Customer
				{
					FirstName = model.FirstName,
					LastName = model.LastName,
					Email = model.Email,
					Phone = model.Phone,
					UserId = user.Id
				};

				_context.Customers.Add(customer);
				await _context.SaveChangesAsync();

				await _signInManager.SignInAsync(user, isPersistent: false);
				return RedirectToAction("Index", "Home");
			}

			foreach (var error in result.Errors)
			{
				ModelState.AddModelError(string.Empty, error.Description);
			}

			return View("~/Views/Account/Register.cshtml", model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Logout()
		{
			await _signInManager.SignOutAsync();
			return RedirectToAction("Index", "Home");
		}
	}
}