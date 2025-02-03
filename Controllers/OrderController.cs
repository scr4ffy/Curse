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
    // �������� �������� ������������ (������ � userId = 1)
    int userId = GetCurrentUserId();
    var user = _context.Users.FirstOrDefault(u => u.Id == userId);

    if (user == null)
    {
        return RedirectToAction("Login", "Account");
    }

    // �������������� ������ ������� �� ����
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
        // �������� �������� ������������
        int userId = GetCurrentUserId();
        var user = _context.Users.FirstOrDefault(u => u.Id == userId);

        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        // ���������� ������ ������������
        user.Pay = model.Pay;
        user.Address = model.Address;
        if (model.Pay == "card")
        {
            user.CardNumber = model.CardNumber;
        }

        _context.SaveChanges();

        TempData["SuccessMessage"] = "��� ����� ������� ��������!";
        return RedirectToAction("Confirmation");
    }

    return View(model);
}

        public IActionResult Confirmation()
        {
            return View(); // ������������� ��� ������������� ������
        }

        // ��������� ����� ��� ��������� �������� ID ������������
        private int GetCurrentUserId()
        {
            return 1; // ������: ���������� ������������� ID ��� ������������
        }
                [HttpGet]
        public IActionResult PlaceOrder()
        {
            // ������������� ������ ������
            var model = new OrderViewModel();
            return View(model);
        }

        [HttpPost]
        public IActionResult PlaceOrder(OrderViewModel model)
        {
            if (ModelState.IsValid)
            {
                // ������ ��� ��������� ������, ��������, ���������� � ���� ������

                // ��������������� �� �������� ������������� ������
                return RedirectToAction("Claim");
            }

            // ���� ������ �� �������, ���������� ����� � ��������
            return View(model);
        }

        // ����� ��� ����������� �������� ������������� ������
        public IActionResult Claim()
        {
            return View();
        }
    }
}
