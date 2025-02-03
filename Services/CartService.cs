using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BikeShop.Data;
using BikeShop.Models;

public class CartService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ApplicationDbContext _context;

    public CartService(IHttpContextAccessor httpContextAccessor, ApplicationDbContext context)
    {
        _httpContextAccessor = httpContextAccessor;
        _context = context;
    }

    public void AddToCart(int bikeId, string name, string imagePath, decimal unitPrice)
{
    // Получаем текущую корзину
    var cart = GetCartItems();

    // Проверяем, есть ли уже товар в корзине
    var cartItem = cart.FirstOrDefault(ci => ci.BikeId == bikeId);

    if (cartItem != null)
    {
        // Если товар уже в корзине, увеличиваем количество
        cartItem.Quantity++;
    }
    else
    {
        // Если товара еще нет в корзине, добавляем его как новый элемент
        cartItem = new CartViewModel
        {
            BikeId = bikeId,
            Name = name,
            ImagePath = imagePath,
            UnitPrice = unitPrice,
            Quantity = 1
        };
        cart.Add(cartItem);
    }

    // Сохраняем обновленную корзину
    SaveCartItems(cart);
}

    public List<CartViewModel> GetCartItems()
    {
        var currentUser = GetCurrentUser();

        if (currentUser != null && !string.IsNullOrEmpty(currentUser.Cart))
        {
            var cartDict = ParseCartString(currentUser.Cart);
            var bikeCatalog = GetCatalogBikes(); // Загружаем каталог велосипедов

            // Создаем список товаров корзины, добавляя информацию из каталога
            return cartDict.Select(ci => 
            {
                var bike = bikeCatalog.FirstOrDefault(b => b.Id == ci.Key);
                return new CartViewModel
                {
                    BikeId = ci.Key,
                    Name = bike?.Name,
                    ImagePath = bike?.ImagePath,
                    UnitPrice = bike?.Price ?? 0,
                    Quantity = ci.Value
                };
            }).ToList();
        }

        // Если пользователь не авторизован, загружаем корзину из сессии
        var session = _httpContextAccessor.HttpContext.Session;
        var cartJson = session.GetString("Cart");
        return cartJson != null
            ? JsonConvert.DeserializeObject<List<CartViewModel>>(cartJson)
            : new List<CartViewModel>();
    }

    private void SaveCartItems(List<CartViewModel> cart)
    {
        var currentUser = GetCurrentUser();

        if (currentUser != null)
        {
            currentUser.Cart = SerializeCart(cart.ToDictionary(ci => ci.BikeId, ci => ci.Quantity));
            _context.SaveChanges();
        }

        _httpContextAccessor.HttpContext.Session.SetString("Cart", JsonConvert.SerializeObject(cart));
    }

    public void UpdateCart(int bikeId, int change)
    {
        var cart = GetCartItems();
        var cartItem = cart.FirstOrDefault(ci => ci.BikeId == bikeId);

        if (cartItem != null)
        {
            cartItem.Quantity += change;

            if (cartItem.Quantity <= 0)
            {
                cart.Remove(cartItem);
            }
        }

        SaveCartItems(cart);
    }

    public void RemoveFromCart(int bikeId)
    {
        var cart = GetCartItems();
        var itemToRemove = cart.FirstOrDefault(i => i.BikeId == bikeId);
        if (itemToRemove != null)
        {
            cart.Remove(itemToRemove);
        }

        SaveCartItems(cart);
    }

    private Dictionary<int, int> ParseCartString(string cartString)
{
    var cart = new Dictionary<int, int>();

    if (string.IsNullOrEmpty(cartString)) return cart;

    // Предполагаем, что каждый идентификатор и количество разделены знаком ':'
    var items = cartString.Split(',');

    foreach (var item in items)
    {
        var parts = item.Split(':');
        if (parts.Length == 2 && int.TryParse(parts[0], out int bikeId) && int.TryParse(parts[1], out int quantity))
        {
            cart[bikeId] = quantity;
        }
    }

    return cart;
}


    private string SerializeCart(Dictionary<int, int> cart)
{
    var cartBuilder = new StringBuilder();

    foreach (var item in cart)
    {
        cartBuilder.Append(item.Key).Append(':').Append(item.Value).Append(',');
    }

    return cartBuilder.ToString().TrimEnd(',');
}


    private User GetCurrentUser()
    {
        var userId = _httpContextAccessor.HttpContext.User.Identity.Name;
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
