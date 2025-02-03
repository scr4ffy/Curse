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
    
    // ���������� ������ � ������ ���������
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

    // ��������� ������ ���������
    public List<WishlistItemViewModel> GetWishlistItems()
    {
        var currentUser = GetCurrentUser();

        // ���� ������������ �����������, �������� ������ ��������� �� ���� ������
        if (currentUser != null && !string.IsNullOrEmpty(currentUser.Want))
        {
            var wishlistIds = ParseWishlistString(currentUser.Want);
            var bikeCatalog = GetCatalogBikes(); // ��������� ������� �����������

            // ������� ������ ������� ������ ���������, �������� ���������� �� ��������
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

        // ����� ��������� �� ������
        var session = _httpContextAccessor.HttpContext.Session;
        var wishlistJson = session.GetString("Wishlist");
        return wishlistJson != null
            ? JsonConvert.DeserializeObject<List<WishlistItemViewModel>>(wishlistJson)
            : new List<WishlistItemViewModel>();
    }

    // ���������� ������ ���������
    private void SaveWishlistItems(List<WishlistItemViewModel> wishlist)
    {
        var currentUser = GetCurrentUser();

        // ���������� ������ � ���� ��� ��������������� ������������
        if (currentUser != null)
        {
            currentUser.Want = SerializeWishlist(wishlist.Select(wi => wi.BikeId).ToList());
            _context.SaveChanges();
        }

        // ���������� ������ � ������
        _httpContextAccessor.HttpContext.Session.SetString("Wishlist", JsonConvert.SerializeObject(wishlist));
    }

    // �������� ������ �� ������ ���������
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

    // �������������� ������ ������ ��������� � ������
    private List<int> ParseWishlistString(string wishlistString)
    {
        return string.IsNullOrEmpty(wishlistString)
            ? new List<int>()
            : wishlistString.Split(',').Select(int.Parse).ToList();
    }

    // ������������ ������ � ������
    private string SerializeWishlist(List<int> wishlist)
    {
        return string.Join(",", wishlist);
    }

    // ��������� �������� ������������
    private User GetCurrentUser()
    {
        var userId = _httpContextAccessor.HttpContext.User.Identity?.Name;
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