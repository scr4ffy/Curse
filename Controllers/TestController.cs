using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BikeShop.Data;
using System.Threading.Tasks;

public class TestController : Controller
{
    private readonly ApplicationDbContext _context;

    public TestController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
{
    try
    {
        var count = await _context.Users.CountAsync();
        return Content($"Количество пользователей: {count}");
    }
    catch (Exception ex)
    {
        return Content($"Произошла ошибка: {ex.Message}");
    }
}

    
}
