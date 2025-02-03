using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using BikeShop.Data;
using BikeShop.Models;

public class WishlistService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ApplicationDbContext _context;

    public WishlistService(IHttpContextAccessor httpContextAccessor, ApplicationDbContext context)
    {
        _httpContextAccessor = httpContextAccessor;
        _context = context;
    }
    
    // Добавление товара в список желаемого
    public void AddToWishlist(int bikeId, string name, string imagePath)
    {
        var wishlist = GetWishlistItems();
        var wishlistItem = wishlist.FirstOrDefault(wi => wi.BikeId == bikeId);

        if (wishlistItem == null)
        {
            wishlistItem = new WishlistItemViewModel
            {
                BikeId = bikeId,
                Name = name,
                ImagePath = imagePath
            };
            wishlist.Add(wishlistItem);
        }

        SaveWishlistItems(wishlist);
    }

    // Получение списка желаемого
    public List<WishlistItemViewModel> GetWishlistItems()
    {
        var currentUser = GetCurrentUser();

        // Если пользователь авторизован, получаем список желаемого из базы данных
        if (currentUser != null && !string.IsNullOrEmpty(currentUser.Want))
        {
            var wishlistIds = ParseWishlistString(currentUser.Want);
            var bikeCatalog = GetCatalogBikes(); // Загружаем каталог велосипедов

            // Создаем список товаров списка желаемого, добавляя информацию из каталога
            return wishlistIds.Select(bikeId => 
            {
                var bike = bikeCatalog.FirstOrDefault(b => b.Id == bikeId);
                return new WishlistItemViewModel
                {
                    BikeId = bikeId,
                    Name = bike?.Name,
                    ImagePath = bike?.ImagePath
                };
            }).ToList();
        }

        // Иначе загружаем из сессии
        var session = _httpContextAccessor.HttpContext.Session;
        var wishlistJson = session.GetString("Wishlist");
        return wishlistJson != null
            ? JsonConvert.DeserializeObject<List<WishlistItemViewModel>>(wishlistJson)
            : new List<WishlistItemViewModel>();
    }

    // Сохранение списка желаемого
    private void SaveWishlistItems(List<WishlistItemViewModel> wishlist)
    {
        var currentUser = GetCurrentUser();

        // Обновление данных в базе для авторизованного пользователя
        if (currentUser != null)
        {
            currentUser.Want = SerializeWishlist(wishlist.Select(wi => wi.BikeId).ToList());
            _context.SaveChanges();
        }

        // Обновление данных в сессии
        _httpContextAccessor.HttpContext.Session.SetString("Wishlist", JsonConvert.SerializeObject(wishlist));
    }

    // Удаление товара из списка желаемого
    public void RemoveFromWishlist(int bikeId)
    {
        var wishlist = GetWishlistItems();
        var itemToRemove = wishlist.FirstOrDefault(wi => wi.BikeId == bikeId);
        if (itemToRemove != null)
        {
            wishlist.Remove(itemToRemove);
        }

        SaveWishlistItems(wishlist);
    }

    // Преобразование строки списка желаемого в список
    private List<int> ParseWishlistString(string wishlistString)
    {
        return string.IsNullOrEmpty(wishlistString)
            ? new List<int>()
            : wishlistString.Split(',').Select(int.Parse).ToList();
    }

    // Сериализация списка в строку
    private string SerializeWishlist(List<int> wishlist)
    {
        return string.Join(",", wishlist);
    }

    // Получение текущего пользователя
    private User GetCurrentUser()
    {
        var userId = _httpContextAccessor.HttpContext.User.Identity?.Name;
        return _context.Users.FirstOrDefault(u => u.Email == userId);
    }

    // Метод для загрузки каталога велосипедов
    private List<Bike> GetCatalogBikes()
    {
        return new List<Bike>
        {
            new Bike { Id = 1, Name = "Горный велосипед", ImagePath = "/images/bike1.jpg", Price = 10000.00m, Description = "Mountain Bike" },
            new Bike { Id = 2, Name = "Городской велосипед", ImagePath = "/images/bike2.jpg", Price = 15000.00m, Description = "Road Bike" },
            new Bike { Id = 3, Name = "Шоссейный велосипед", ImagePath = "/images/bike3.jpg", Price = 20000.00m, Description = "Hybrid Bike" }
        };
    }
}