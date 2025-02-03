// Models/CartItemViewModel.cs
namespace BikeShop.Models
{
    public class CartViewModel
    {
        public int BikeId { get; set; }
        public string Name { get; set; }
        public string ImagePath { get; set; } // ���� � ����������� ������
        public decimal UnitPrice { get; set; } // ���� �� �������
        public int Quantity { get; set; } // ����������
        public decimal TotalPrice => UnitPrice * Quantity;
    }
}
