public class CartItem
{
    public int Id { get; set; }
    public int BikeId { get; set; }
    public string UserId { get; set; } // ���������, ��� ��� ������
    public int Quantity { get; set; }
    public string Name { get; set; }
    public string ImagePath { get; set; }
    public decimal UnitPrice { get; set; }
}
