// Controllers/CartController.cs
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using BikeShop.Models;

public class CartController : Controller
{
    private readonly CartService _cartService;

    public CartController(CartService cartService)
    {
        _cartService = cartService;
    }

    // ���������� ������ � �������
    [HttpPost]
    public IActionResult AddToCart(int bikeId, string name, string imagePath, decimal unitPrice)
    {
        _cartService.AddToCart(bikeId, name, imagePath, unitPrice);
        TempData["SuccessMessage"] = "����� ������� �������� � �������.";
    
        return RedirectToAction("Index");
    }

    // ����������� �������
    public IActionResult Index()
    {
        var cartItems = _cartService.GetCartItems();
        return View(cartItems);
    }

    // ���������� ���������� ������ � �������
    [HttpPost]
    public IActionResult UpdateQuantity(int bikeId, string action)
    {
        if (action == "increase")
        {
            _cartService.UpdateCart(bikeId, 1); // ����������� ���������� �� 1
            TempData["SuccessMessage"] = "���������� ������ ���������.";
        }
        else if (action == "decrease")
        {
            _cartService.UpdateCart(bikeId, -1); // ��������� ���������� �� 1
            TempData["SuccessMessage"] = "���������� ������ ���������.";
        }

        return RedirectToAction("Index");
    }

    // �������� ������ �� �������
    public IActionResult RemoveFromCart(int bikeId)
    {
        _cartService.RemoveFromCart(bikeId);
        TempData["SuccessMessage"] = "����� ������� ������ �� �������."; // ����������� �� �������� ��������
        return RedirectToAction("Index");
    }

    // ���������� ������
    public IActionResult Checkout()
{
    var cartItems = _cartService.GetCartItems(); // �������� ������ �� �������

    // ��������, ��� ������� �� �����
    if (!cartItems.Any())
    {
        TempData["ErrorMessage"] = "������� �����. �������� ������ ����� ����������� ������.";
        return RedirectToAction("Index");
    }

    var orderViewModel = new OrderViewModel
    {
        CartItems = cartItems.Select(item => new CartViewModel
        {
            ImagePath = item.ImagePath,
            Name = item.Name,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice,
            // TotalPrice �� ��������������� ��������, � �������������� ��� ��������� � ��������
        }).ToList()
    };

    return View(orderViewModel); // �������� ������ ��� �����������
}

}