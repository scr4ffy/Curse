using Microsoft.EntityFrameworkCore;
using BikeShop.Models;
public class BikeShopContext : DbContext
{
    public BikeShopContext(DbContextOptions<BikeShopContext> options) : base(options)
    {
    }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Bike> Bikes { get; set; }
}
