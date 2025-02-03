using Microsoft.AspNetCore.Mvc;
using System.Linq;
using BikeShop.Data;
using BikeShop.Models;

namespace BikeShop.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
public IActionResult Checkout()
{
    // Получить текущего пользователя (пример с userId = 1)
    int userId = GetCurrentUserId();
    var user = _context.Users.FirstOrDefault(u => u.Id == userId);

    if (user == null)
    {
        return RedirectToAction("Login", "Account");
    }

    // Предзаполнение модели данными из базы
    var model = new OrderViewModel
    {
        Pay = user.Pay,
        Address = user.Address,
        CardNumber = user.Pay == "card" ? user.CardNumber : null
    };

    return View(model);
}


        [HttpPost]
public IActionResult Checkout(OrderViewModel model)
{
    if (ModelState.IsValid)
    {
        // Получить текущего пользователя
        int userId = GetCurrentUserId();
        var user = _context.Users.FirstOrDefault(u => u.Id == userId);

        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        // Обновление данных пользователя
        user.Pay = model.Pay;
        user.Address = model.Address;
        if (model.Pay == "card")
        {
            user.CardNumber = model.CardNumber;
        }

        _context.SaveChanges();

        TempData["SuccessMessage"] = "Ваш заказ успешно оформлен!";
        return RedirectToAction("Confirmation");
    }

    return View(model);
}

        public IActionResult Confirmation()
        {
            return View(); // Представление для подтверждения заказа
        }

        // Временный метод для получения текущего ID пользователя
        private int GetCurrentUserId()
        {
            return 1; // Пример: возвращаем фиксированный ID для тестирования
        }
                [HttpGet]
        public IActionResult PlaceOrder()
        {
            // Инициализация модели заказа
            var model = new OrderViewModel();
            return View(model);
        }

        [HttpPost]
        public IActionResult PlaceOrder(OrderViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Логика для обработки заказа, например, сохранение в базе данных

                // Перенаправление на страницу подтверждения заказа
                return RedirectToAction("Claim");
            }

            // Если модель не валидна, возвращаем форму с ошибками
            return View(model);
        }

        // Метод для отображения страницы подтверждения заказа
        public IActionResult Claim()
        {
            return View();
        }
    }
}
