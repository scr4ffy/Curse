// Data/ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using BikeShop.Models;

namespace BikeShop.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    public DbSet<User> Users { get; set; }
    public DbSet<Bike> Bikes { get; set; } 
    public DbSet<CartItem> CartItems { get; set; }
    }
}
