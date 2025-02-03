// Models/CartItemViewModel.cs
namespace BikeShop.Models
{
    public class CartViewModel
    {
        public int BikeId { get; set; }
        public string Name { get; set; }
        public string ImagePath { get; set; } // путь к изображению товара
        public decimal UnitPrice { get; set; } // цена за единицу
        public int Quantity { get; set; } // количество
        public decimal TotalPrice => UnitPrice * Quantity;
    }
}
