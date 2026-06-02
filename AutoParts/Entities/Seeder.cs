using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AutoParts.Models;
using AutoParts.Data;

namespace AutoParts.Entities
{
	public static class Seeder
	{
		public static async Task SeedDataAsync(IServiceProvider services, IWebHostEnvironment env, IConfiguration config)
		{
			var roleManager = services.GetRequiredService<RoleManager<DbRole>>();
			var userManager = services.GetRequiredService<UserManager<DbUser>>();
			var context = services.GetRequiredService<ApplicationDbContext>();

			string[] roleNames = { "Manager", "Client" };
			foreach (var roleName in roleNames)
			{
				if (!await roleManager.RoleExistsAsync(roleName))
				{
					await roleManager.CreateAsync(new DbRole { Name = roleName });
				}
			}

			string adminEmail = "burian_ak21@nuwm.edu.ua";
			var adminUser = await userManager.FindByEmailAsync(adminEmail);
			if (adminUser == null)
			{
				DbUser newAdmin = new DbUser
				{
					UserName = adminEmail,
					Email = adminEmail,
					FirstName = "Даша",
					LastName = "Дем'янюк",
					EmailConfirmed = true
				};

				var createPowerUser = await userManager.CreateAsync(newAdmin, "123456");
				if (createPowerUser.Succeeded)
				{
					await userManager.AddToRoleAsync(newAdmin, "Manager");
				}
			}

			if (!await context.Categories.AnyAsync())
			{
				await context.Categories.AddRangeAsync(
					new Category { Name = "Двигун", ImageUrl = "http://cdn-icons-png.flaticon.com/512/2061/2061910.png" },
					new Category { Name = "Гальма", ImageUrl = "https://cdn-icons-png.flaticon.com/512/2061/2061915.png" },
					new Category { Name = "Підвіска", ImageUrl = "https://cdn-icons-png.flaticon.com/512/5556/5556143.png" },
					new Category { Name = "Фільтри", ImageUrl = "https://cdn-icons-png.flaticon.com/512/13507/13507865.png" },
					new Category { Name = "Світло", ImageUrl = "https://cdn-icons-png.flaticon.com/512/2061/2061937.png" },
					new Category { Name = "Електрика", ImageUrl = "https://cdn-icons-png.flaticon.com/512/4276/4276100.png" },
					new Category { Name = "Охолодження", ImageUrl = "https://cdn-icons-png.flaticon.com/512/3626/3626836.png" },
					new Category { Name = "Кузов", ImageUrl = "https://cdn-icons-png.flaticon.com/512/13584/13584120.png" },
					new Category { Name = "Трансмісія", ImageUrl = "https://cdn-icons-png.flaticon.com/512/1170/1170437.png" },
					new Category { Name = "Рульове управління", ImageUrl = "https://cdn-icons-png.flaticon.com/512/2783/2783749.png" }
				);
				await context.SaveChangesAsync();
			}

			if (!await context.Brands.AnyAsync())
			{
				await context.Brands.AddRangeAsync(
					new Brand { Name = "Bosch", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/c/c3/Bosch_logo.png" },
					new Brand { Name = "Brembo", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/9/9f/Brembo_logo.png" },
					new Brand { Name = "KYB", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/7/7b/KYB_Corporation_company_logo.svg/3840px-KYB_Corporation_company_logo.svg.png" },
					new Brand { Name = "Valeo", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/2/2b/Valeo_Logo.svg/3840px-Valeo_Logo.svg.png" },
					new Brand { Name = "Varta", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/1/13/Varta-Logo.svg/3840px-Varta-Logo.svg.png" },
					new Brand { Name = "Mobil", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/3/3d/Mobil_logo.svg/3840px-Mobil_logo.svg.png" },
					new Brand { Name = "Fram", ImageUrl = "https://massive.ua/image/brand_logo/big/fram.png" },
					new Brand { Name = "NGK", ImageUrl = "https://cdn.freebiesupply.com/logos/large/2x/ngk-logo-png-transparent.png" },
					new Brand { Name = "Mann-Filter", ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSBGZH1jlQwYeNMdtyyWUHZy-xQy4V2RHW2ug&s" },
					new Brand { Name = "TRW", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/d/db/TRW_logo.svg/3840px-TRW_logo.svg.png" },
					new Brand { Name = "Denso", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/a/a7/Denso_logo.svg/3840px-Denso_logo.svg.png" },
					new Brand { Name = "Sachs", ImageUrl = "https://1000logos.net/wp-content/uploads/2020/10/Sachs-Logo.png" }
				);
				await context.SaveChangesAsync();
			}

			if (!await context.Makes.AnyAsync())
			{
				var opel = new Make { Name = "Opel" };
				var bmw = new Make { Name = "BMW" };
				var toyota = new Make { Name = "Toyota" };

				var insignia = new Model { Name = "Insignia" };
				var x5 = new Model { Name = "X5" };
				var camry = new Model { Name = "Camry" };

				var hatchback = new BodyType { Name = "Хетчбек" };
				var suv = new BodyType { Name = "Позашляховик" };
				var sedan = new BodyType { Name = "Седан" };

				var engine14 = new Engine { Name = "1.4 бензин" };
				var engine30 = new Engine { Name = "3.0 дизель" };
				var engine25 = new Engine { Name = "2.5 гібрид" };

				await context.Makes.AddRangeAsync(opel, bmw, toyota);
				await context.Models.AddRangeAsync(insignia, x5, camry);
				await context.BodyTypes.AddRangeAsync(hatchback, suv, sedan);
				await context.Engines.AddRangeAsync(engine14, engine30, engine25);
				await context.SaveChangesAsync();

				var opelCar = new Vehicle { Year = 2010, Make = opel, Model = insignia, BodyType = hatchback, Engine = engine14 };
				var bmwCar = new Vehicle { Year = 2015, Make = bmw, Model = x5, BodyType = suv, Engine = engine30 };
				var toyotaCar = new Vehicle { Year = 2018, Make = toyota, Model = camry, BodyType = sedan, Engine = engine25 };

				await context.Vehicles.AddRangeAsync(opelCar, bmwCar, toyotaCar);
				await context.SaveChangesAsync();
			}

			if (!await context.AutoParts.AnyAsync())
			{
				var categories = await context.Categories.ToDictionaryAsync(c => c.Name, c => c.Id);
				var brands = await context.Brands.ToDictionaryAsync(b => b.Name, b => b.Id);

				var opelCar = await context.Vehicles.FirstOrDefaultAsync(v => v.Make.Name == "Opel");
				var bmwCar = await context.Vehicles.FirstOrDefaultAsync(v => v.Make.Name == "BMW");
				var toyotaCar = await context.Vehicles.FirstOrDefaultAsync(v => v.Make.Name == "Toyota");

				if (opelCar != null && bmwCar != null && toyotaCar != null)
				{
					var parts = new List<AutoPart>
					{
						new AutoPart { SKU = "FLT-001", Name = "Масляний фільтр", Price = 350m, StockQuantity = 50, CategoryId = categories["Фільтри"], BrandId = brands["Bosch"], ImageUrls = new List<string> { "https://content1.rozetka.com.ua/goods/images/big/662276176.jpg", "https://content.rozetka.com.ua/goods/images/big/662276186.jpg", "https://content.rozetka.com.ua/goods/images/big/662276189.jpg" }, Vehicles = new List<Vehicle> { opelCar, bmwCar, toyotaCar } },
						new AutoPart { SKU = "FLT-002", Name = "Повітряний фільтр", Price = 420m, StockQuantity = 30, CategoryId = categories["Фільтри"], BrandId = brands["Mann-Filter"], ImageUrls = new List<string> { "https://content2.rozetka.com.ua/goods/images/big/654931972.webp" }, Vehicles = new List<Vehicle> { opelCar } },
						new AutoPart { SKU = "FLT-003", Name = "Салонний фільтр вугільний", Price = 580m, StockQuantity = 15, CategoryId = categories["Фільтри"], BrandId = brands["Fram"], ImageUrls = new List<string> { "https://content.rozetka.com.ua/goods/images/big/545104947.jpg" }, Vehicles = new List<Vehicle> { bmwCar, toyotaCar } },

						new AutoPart { SKU = "BRK-001", Name = "Гальмівні колодки передні", Price = 1200m, StockQuantity = 20, CategoryId = categories["Гальма"], BrandId = brands["Brembo"],ImageUrls = new List<string> { "https://content1.rozetka.com.ua/goods/images/big/654439394.jpg", "https://content.rozetka.com.ua/goods/images/big/654439399.jpg" }, Vehicles = new List<Vehicle> { opelCar, bmwCar } },
						new AutoPart { SKU = "BRK-002", Name = "Гальмівний диск", Price = 2100m, StockQuantity = 10, CategoryId = categories["Гальма"], BrandId = brands["TRW"], ImageUrls = new List<string> {"https://content1.rozetka.com.ua/goods/images/big_tile/563192147.jpg" }, Vehicles = new List<Vehicle> { toyotaCar } },
						new AutoPart { SKU = "BRK-003", Name = "Гальмівна рідина DOT4 1л", Price = 320m, StockQuantity = 100, CategoryId = categories["Гальма"], BrandId = brands["Bosch"], ImageUrls = new List<string> {"https://content.rozetka.com.ua/goods/images/big/233857762.jpg" }, Vehicles = new List<Vehicle> { opelCar, bmwCar, toyotaCar } },

						new AutoPart { SKU = "SUS-001", Name = "Амортизатор передній газомасляний", Price = 2450m, StockQuantity = 12, CategoryId = categories["Підвіска"], BrandId = brands["KYB"], ImageUrls = new List<string> {"https://content2.rozetka.com.ua/goods/images/big/469019845.jpg" }, Vehicles = new List<Vehicle> { opelCar } },
						new AutoPart { SKU = "SUS-002", Name = "Амортизатор задній", Price = 3100m, StockQuantity = 8, CategoryId = categories["Підвіска"], BrandId = brands["Sachs"], ImageUrls = new List<string> {"https://content.rozetka.com.ua/goods/images/big_tile/299476062.jpg" }, Vehicles = new List<Vehicle> { bmwCar } },
						new AutoPart { SKU = "SUS-003", Name = "Стійка стабілізатора", Price = 450m, StockQuantity = 40, CategoryId = categories["Підвіска"], BrandId = brands["TRW"], ImageUrls = new List<string> {"https://content2.rozetka.com.ua/goods/images/big/603627627.jpg"}, Vehicles = new List<Vehicle> { toyotaCar, opelCar } },

						new AutoPart { SKU = "ENG-001", Name = "Свічка запалювання Iridium", Price = 480m, StockQuantity = 60, CategoryId = categories["Електрика"], BrandId = brands["NGK"], ImageUrls = new List<string> {"https://content2.rozetka.com.ua/goods/images/big/591699745.jpg" }, Vehicles = new List<Vehicle> { opelCar, toyotaCar } },
						new AutoPart { SKU = "ENG-002", Name = "Свічка накалювання", Price = 650m, StockQuantity = 24, CategoryId = categories["Електрика"], BrandId = brands["Denso"], ImageUrls = new List<string> {"https://content2.rozetka.com.ua/goods/images/big/581890070.jpg" }, Vehicles = new List<Vehicle> { bmwCar } },
						new AutoPart { SKU = "ENG-003", Name = "Акумулятор Silver Dynamic 77Ah", Price = 4500m, StockQuantity = 5, CategoryId = categories["Електрика"], BrandId = brands["Varta"], ImageUrls = new List<string> {"https://content2.rozetka.com.ua/goods/images/big/586504226.png" }, Vehicles = new List<Vehicle> { bmwCar, toyotaCar } },

						new AutoPart { SKU = "OIL-001", Name = "Моторна олива Super 3000 5W-40 4л", Price = 1150m, StockQuantity = 35, CategoryId = categories["Двигун"], BrandId = brands["Mobil"], ImageUrls = new List<string> {"https://content1.rozetka.com.ua/goods/images/big/475790004.jpg" }, Vehicles = new List<Vehicle> { opelCar, bmwCar, toyotaCar } },
						new AutoPart { SKU = "ENG-004", Name = "Комплект ременя ГРМ", Price = 2800m, StockQuantity = 6, CategoryId = categories["Двигун"], BrandId = brands["Bosch"], ImageUrls = new List<string> {"https://content.rozetka.com.ua/goods/images/big/523919398.jpg" }, Vehicles = new List<Vehicle> { opelCar } },

						new AutoPart { SKU = "LGT-001", Name = "Лампа ксенонова D1S", Price = 1850m, StockQuantity = 14, CategoryId = categories["Світло"], BrandId = brands["Bosch"], ImageUrls = new List<string> {"https://content1.rozetka.com.ua/goods/images/big_tile/592443612.jpg" }, Vehicles = new List<Vehicle> { bmwCar } },
						new AutoPart { SKU = "LGT-002", Name = "Лампа галогенна H7", Price = 220m, StockQuantity = 80, CategoryId = categories["Світло"], BrandId = brands["Valeo"], ImageUrls = new List<string> {"https://img.dok.ua/images/tile/product/1220/15/32229083_3.jpg" }, Vehicles = new List<Vehicle> { opelCar, toyotaCar } },

						new AutoPart { SKU = "COL-001", Name = "Антифриз червоний G12 5л", Price = 750m, StockQuantity = 25, CategoryId = categories["Охолодження"], BrandId = brands["Mobil"], ImageUrls = new List<string> {"https://img.autoklad.ua/imgbank/Image/pic/mobil_145723.jpg" }, Vehicles = new List<Vehicle> { opelCar, bmwCar, toyotaCar } },
						new AutoPart { SKU = "COL-002", Name = "Радіатор охолодження", Price = 4200m, StockQuantity = 3, CategoryId = categories["Охолодження"], BrandId = brands["Valeo"], ImageUrls = new List<string> {"https://content2.rozetka.com.ua/goods/images/big_tile/582216459.jpg" }, Vehicles = new List<Vehicle> { toyotaCar } },

						new AutoPart { SKU = "STR-001", Name = "Кермова тяга", Price = 1100m, StockQuantity = 18, CategoryId = categories["Рульове управління"], BrandId = brands["TRW"], ImageUrls = new List<string> {"https://img.dok.ua/images/tile/product/0320/13/1214793_1.jpg" }, Vehicles = new List<Vehicle> { opelCar, bmwCar } },
						new AutoPart { SKU = "BDY-001", Name = "Щітки склоочисника (комплект)", Price = 890m, StockQuantity = 30, CategoryId = categories["Кузов"], BrandId = brands["Bosch"], ImageUrls = new List<string> {"https://content.rozetka.com.ua/goods/images/big_tile/163416172.jpg" }, Vehicles = new List<Vehicle> { opelCar, bmwCar, toyotaCar } }
					};

					await context.AutoParts.AddRangeAsync(parts);
					await context.SaveChangesAsync();
				}
			}
		}
	}
}