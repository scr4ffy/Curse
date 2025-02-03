using Microsoft.AspNetCore.Mvc;
using BikeShop.Models;

public class WishlistController : Controller
{
    private readonly WishlistService _wishlistService;

    public WishlistController(WishlistService wishlistService)
    {
        _wishlistService = wishlistService;
    }

    // ���������� ������ � ������ ���������
    [HttpPost]
    public IActionResult AddToWishlist(int bikeId, string name, string imagePath)
    {
        _wishlistService.AddToWishlist(bikeId, name, imagePath);
        TempData["SuccessMessage"] = "����� ������� �������� � ������ ���������.";
        return RedirectToAction("Index");
    }

    // ����������� ������ ���������
    public IActionResult Index()
    {
        var wishlistItems = _wishlistService.GetWishlistItems();
        return View(wishlistItems);
    }

    // �������� ������ �� ������ ���������
    public IActionResult RemoveFromWishlist(int bikeId)
    {
        _wishlistService.RemoveFromWishlist(bikeId);
        TempData["SuccessMessage"] = "����� ������� ������ �� ������ ���������.";
        return RedirectToAction("Index");
    }
}