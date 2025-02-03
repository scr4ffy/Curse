namespace BikeShop.Models
{
    public class OrderViewModel
    {
        public List<CartViewModel> CartItems { get; set; } = new List<CartViewModel>();
        public decimal TotalAmount => CartItems.Sum(item => item.TotalPrice); // Итоговая сумма заказа

        public string Pay { get; set; }
        public string DeliveryMethod { get; set; }
        public string Address { get; set; }
        public string Comment { get; set; }
        public string? CardNumber { get; set; } 
        public OrderViewModel()
    {
        CartItems = new List<CartViewModel>();
    }
    }
    
}
