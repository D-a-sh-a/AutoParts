using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AutoParts.Entities;
using AutoParts.Models;
using System.Reflection.Emit;

namespace AutoParts.Data
{
	public class ApplicationDbContext : IdentityDbContext<DbUser, DbRole, int>
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
			: base(options)
		{
		}

		public DbSet<Category> Categories { get; set; }
		public DbSet<AutoPart> AutoParts { get; set; }
		public DbSet<Order> Orders { get; set; }
		public DbSet<OrderItem> OrderItems { get; set; }
		public DbSet<Make> Makes { get; set; }
		public DbSet<Model> Models { get; set; }
		public DbSet<BodyType> BodyTypes { get; set; }
		public DbSet<Engine> Engines { get; set; }
		public DbSet<Vehicle> Vehicles { get; set; }
		public DbSet<Brand> Brands { get; set; }

		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);

			builder.Entity<Order>()
				.Property(o => o.Status)
				.HasConversion<string>();

			builder.Entity<AutoPart>().Property(p => p.Price).HasPrecision(18, 2);
			builder.Entity<Order>().Property(o => o.TotalAmount).HasPrecision(18, 2);
			builder.Entity<OrderItem>().Property(oi => oi.UnitPrice).HasPrecision(18, 2);
		}
	}
}