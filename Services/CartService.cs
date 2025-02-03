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
    // �������� ������� �������
    var cart = GetCartItems();

    // ���������, ���� �� ��� ����� � �������
    var cartItem = cart.FirstOrDefault(ci => ci.BikeId == bikeId);

    if (cartItem != null)
    {
        // ���� ����� ��� � �������, ����������� ����������
        cartItem.Quantity++;
    }
    else
    {
        // ���� ������ ��� ��� � �������, ��������� ��� ��� ����� �������
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

    // ��������� ����������� �������
    SaveCartItems(cart);
}

    public List<CartViewModel> GetCartItems()
    {
        var currentUser = GetCurrentUser();

        if (currentUser != null && !string.IsNullOrEmpty(currentUser.Cart))
        {
            var cartDict = ParseCartString(currentUser.Cart);
            var bikeCatalog = GetCatalogBikes(); // ��������� ������� �����������

            // ������� ������ ������� �������, �������� ���������� �� ��������
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

        // ���� ������������ �� �����������, ��������� ������� �� ������
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

    // ������������, ��� ������ ������������� � ���������� ��������� ������ ':'
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

    // ����� ��� �������� �������� �����������
    private List<Bike> GetCatalogBikes()
    {
        return new List<Bike>
        {
            new Bike { Id = 1, Name = "������ ���������", ImagePath = "/images/bike1.jpg", Price = 10000.00m, Description = "Mountain Bike" },
            new Bike { Id = 2, Name = "��������� ���������", ImagePath = "/images/bike2.jpg", Price = 15000.00m, Description = "Road Bike" },
            new Bike { Id = 3, Name = "��������� ���������", ImagePath = "/images/bike3.jpg", Price = 20000.00m, Description = "Hybrid Bike" }
        };
    }
}
