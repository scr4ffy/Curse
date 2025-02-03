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

    // Добавление товара в корзину
    [HttpPost]
    public IActionResult AddToCart(int bikeId, string name, string imagePath, decimal unitPrice)
    {
        _cartService.AddToCart(bikeId, name, imagePath, unitPrice);
        TempData["SuccessMessage"] = "Товар успешно добавлен в корзину.";
    
        return RedirectToAction("Index");
    }

    // Отображение корзины
    public IActionResult Index()
    {
        var cartItems = _cartService.GetCartItems();
        return View(cartItems);
    }

    // Обновление количества товара в корзине
    [HttpPost]
    public IActionResult UpdateQuantity(int bikeId, string action)
    {
        if (action == "increase")
        {
            _cartService.UpdateCart(bikeId, 1); // Увеличиваем количество на 1
            TempData["SuccessMessage"] = "Количество товара увеличено.";
        }
        else if (action == "decrease")
        {
            _cartService.UpdateCart(bikeId, -1); // Уменьшаем количество на 1
            TempData["SuccessMessage"] = "Количество товара уменьшено.";
        }

        return RedirectToAction("Index");
    }

    // Удаление товара из корзины
    public IActionResult RemoveFromCart(int bikeId)
    {
        _cartService.RemoveFromCart(bikeId);
        TempData["SuccessMessage"] = "Товар успешно удален из корзины."; // Уведомление об успешном удалении
        return RedirectToAction("Index");
    }

    // Оформление заказа
    public IActionResult Checkout()
{
    var cartItems = _cartService.GetCartItems(); // Получаем товары из корзины

    // Проверка, что корзина не пуста
    if (!cartItems.Any())
    {
        TempData["ErrorMessage"] = "Корзина пуста. Добавьте товары перед оформлением заказа.";
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
            // TotalPrice не устанавливается напрямую, а рассчитывается при обращении к свойству
        }).ToList()
    };

    return View(orderViewModel); // Передаем модель для отображения
}

}