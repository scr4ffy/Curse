namespace BikeShop.Models
{
    public class Bike
    {
        public int Id { get; set; }
        public string ImagePath { get; set; } // Путь к изображению
        public string Name { get; set; }      // Название велосипеда
        public decimal Price { get; set; }    // Цена велосипеда
        public string Description { get; set; } // Описание велосипеда
        public string Category { get; set; }
    }
}
