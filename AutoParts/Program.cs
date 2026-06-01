using AutoParts.Data;
using AutoParts.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<DbUser, DbRole>(options =>
{
	options.Password.RequireDigit = false;
	options.Password.RequireLowercase = false;
	options.Password.RequireUppercase = false;
	options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddSession(options =>
{
	options.IdleTimeout = TimeSpan.FromMinutes(30);
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseDeveloperExceptionPage();
}
else
{
	app.UseExceptionHandler("/Home/Error");
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
	var services = scope.ServiceProvider;
	try
	{
		await Seeder.SeedDataAsync(services, app.Environment, builder.Configuration);
	}
	catch (Exception ex)
	{
		var logger = services.GetRequiredService<ILogger<Program>>();
		logger.LogError(ex, "Помилка під час наповнення бази даних (Seeding).");
	}
}

app.Run();