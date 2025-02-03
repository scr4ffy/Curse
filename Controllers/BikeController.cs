using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using BikeShop.Models;
using BikeShop.Data;

namespace BikeShop.Controllers
{
    public class CatalogController : Controller
    {
        private static List<Bike> bikes = new List<Bike>
        {
            new Bike { Id = 1, Name = "������ ���������", Price = 10000, ImagePath = "/images/bike1.jpg", Description = "�������� ������� ����������", Category = "������" },
            new Bike { Id = 2, Name = "��������� ���������", Price = 15000, ImagePath = "/images/bike2.jpg", Description = "�������� ���������� ����������", Category = "���������" },
            new Bike { Id = 3, Name = "������� ���������", Price = 5000, ImagePath = "/images/bike3.jpg", Description = "�������� �������� ����������", Category = "�������" }
        };

        private readonly ApplicationDbContext _context;

        public CatalogController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ����� ��� ����������� ��������
        public IActionResult Index(string searchQuery, decimal? minPrice, decimal? maxPrice, string category)
{
    var filteredBikes = bikes;

    // ���������� �� ���������� �������
    if (!string.IsNullOrEmpty(searchQuery))
    {
        filteredBikes = filteredBikes
            .Where(b => b.Name.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                        b.Description.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    // ���������� �� ����������� � ������������ ����
    if (minPrice.HasValue)
    {
        filteredBikes = filteredBikes.Where(b => b.Price >= minPrice.Value).ToList();
    }
    if (maxPrice.HasValue)
    {
        filteredBikes = filteredBikes.Where(b => b.Price <= maxPrice.Value).ToList();
    }

    // ���������� �� ���������
    if (!string.IsNullOrEmpty(category))
    {
        filteredBikes = filteredBikes.Where(b => b.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    // �������� ��������� � �������������
    ViewData["SearchQuery"] = searchQuery;
    ViewData["MinPrice"] = minPrice;
    ViewData["MaxPrice"] = maxPrice;
    ViewData["Category"] = category;

    return View(filteredBikes);
}


        // ����� ��� ����������� ������� ����������
        [HttpGet("Catalog/Details/{id}")]
        public IActionResult Details(int id)
        {
            var bike = bikes.SingleOrDefault(b => b.Id == id);
            if (bike == null)
            {
                return NotFound(); // ���� ��������� �� ������
            }
            return View(bike); // ������� ���������� ��������� � �������������
        }

        [HttpPost]
        public IActionResult AddToWishlist(int bikeId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return RedirectToAction("Index", "Catalog");
            }

            var wishlist = user.Want?.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();
            if (!wishlist.Contains(bikeId.ToString()))
            {
                wishlist.Add(bikeId.ToString());
                user.Want = string.Join(",", wishlist);
                _context.SaveChanges();
            }

            return RedirectToAction("Index", "Catalog");
        }

        private int GetCurrentUserId()
        {
            return 1; // �������� ���������� 1 ��� �������
        }
    }
}
