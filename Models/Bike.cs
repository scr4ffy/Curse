namespace BikeShop.Models
{
    public class Bike
    {
        public int Id { get; set; }
        public string ImagePath { get; set; } // ���� � �����������
        public string Name { get; set; }      // �������� ����������
        public decimal Price { get; set; }    // ���� ����������
        public string Description { get; set; } // �������� ����������
        public string Category { get; set; }
    }
}
