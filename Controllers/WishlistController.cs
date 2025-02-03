using Microsoft.AspNetCore.Mvc;
using BikeShop.Models;

public class WishlistController : Controller
{
    private readonly WishlistService _wishlistService;

    public WishlistController(WishlistService wishlistService)
    {
        _wishlistService = wishlistService;
    }

    // ƒобавление товара в список желаемого
    [HttpPost]
    public IActionResult AddToWishlist(int bikeId, string name, string imagePath)
    {
        _wishlistService.AddToWishlist(bikeId, name, imagePath);
        TempData["SuccessMessage"] = "“овар успешно добавлен в список желаемого.";
        return RedirectToAction("Index");
    }

    // ќтображение списка желаемого
    public IActionResult Index()
    {
        var wishlistItems = _wishlistService.GetWishlistItems();
        return View(wishlistItems);
    }

    // ”даление товара из списка желаемого
    public IActionResult RemoveFromWishlist(int bikeId)
    {
        _wishlistService.RemoveFromWishlist(bikeId);
        TempData["SuccessMessage"] = "“овар успешно удален из списка желаемого.";
        return RedirectToAction("Index");
    }
}