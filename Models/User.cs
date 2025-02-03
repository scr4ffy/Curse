namespace BikeShop.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ProfileImagePath { get; set; }
        public string? Cart { get; set; }
        public string? Want { get; set; }
        public string? Address { get; set; }
        public string? Pay { get; set; }
        public string? CardNumber { get; set; } 
    }
    public class RegisterViewModel
{
    public string Email { get; set; }
    public string Password { get; set; }
}
    public class CartItem
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int BikeId { get; set; }
    public int Quantity { get; set; }
    public DateTime CreatedAt { get; set; }

    // Навигационные свойства
    public User User { get; set; }
    public Bike Bike { get; set; } // Изменено на Bike
}

    public class Item
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }
}
