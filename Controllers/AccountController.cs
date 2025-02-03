using Microsoft.AspNetCore.Mvc;
using BikeShop.Data;
using BikeShop.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Text.Json;

namespace BikeShop.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
[HttpPost]
public async Task<IActionResult> Register(RegisterViewModel model)
{
    if (ModelState.IsValid)
    {
        // Проверка на корректность email через регулярное выражение с точкой перед доменом и только английские символы
        var emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.(ru|com|net)$";
        if (!System.Text.RegularExpressions.Regex.IsMatch(model.Email, emailPattern))
        {
            ModelState.AddModelError("Email", "Email должен быть в формате @(почта).ru, .com или .net и содержать только латинские символы.");
            TempData["ErrorMessage"] = "Некорректный формат Email или используется не латинский алфавит.";
            return View(model);
        }
                if (model.Password.Length < 6)
        {
            ModelState.AddModelError("Password", "Пароль должен содержать минимум 6 символов.");
            TempData["ErrorMessage"] = "Пароль слишком короткий. Должно быть минимум 6 символов.";
            return View(model);
        }

        if (await _context.Users.AnyAsync(u => u.Email == model.Email))
        {
            ModelState.AddModelError(string.Empty, "Пользователь с таким email уже существует.");
            TempData["ErrorMessage"] = "Пользователь с таким email уже существует.";
            return View(model);
        
        }

        var user = new User
        {
            Email = model.Email,
            Password = HashPassword(model.Password)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Регистрация прошла успешно. Пожалуйста, войдите в систему.";
        return RedirectToAction("Login");
    }

    TempData["ErrorMessage"] = "Ошибка регистрации. Пожалуйста, проверьте введенные данные.";
    return View(model);
}
        // GET: /Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == email);
            if (user != null && VerifyPassword(password, user.Password))
            {
                // Установка куки аутентификации
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Email),
                    new Claim("UserId", user.Id.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                TempData["SuccessMessage"] = "Вы успешно вошли в систему.";
                return RedirectToAction("Profile");
            }

            ModelState.AddModelError(string.Empty, "Неверный email или пароль.");
            TempData["ErrorMessage"] = "Ошибка входа. Пожалуйста, проверьте введенные данные.";
            return View();
        }

        // GET: /Account/Profile
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            // Проверка, что User.Identity не null и является аутентифицированным
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login");
            }

            var email = User.Identity.Name; // Теперь мы уверены, что это не null
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == email);
            
            if (user == null)
            {
                TempData["ErrorMessage"] = "Пользователь не найден.";
                return RedirectToAction("Login");
            }

            var profileImagePath = string.IsNullOrEmpty(user.ProfileImagePath) 
                ? "profile.png" 
                : user.ProfileImagePath;

            ViewBag.ProfileImagePath = profileImagePath;

            return View(user);
        }

        // POST: /Account/UploadProfileImage
        [HttpPost]
        public async Task<IActionResult> UploadProfileImage(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                var userIdString = User.FindFirstValue("UserId"); // Получаем ID пользователя из claims
                if (int.TryParse(userIdString, out int userId)) // Преобразуем строку в int
                {
                    var user = await _context.Users.FindAsync(userId);

                    if (user != null)
                    {
                        // Генерация уникального имени файла
                        var fileName = $"{userId}_{Path.GetFileName(file.FileName)}";
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                        // Сохранение файла в папку wwwroot/images
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        // Сохранение пути к файлу в профиле пользователя
                        user.ProfileImagePath = fileName;
                        await _context.SaveChangesAsync();

                        TempData["SuccessMessage"] = "Фото профиля успешно загружено.";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Пользователь не найден.";
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Ошибка при преобразовании ID пользователя.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Ошибка загрузки фото.";
            }

            return RedirectToAction("Profile"); // Перенаправление на представление профиля
        }

        // POST: /Account/Logout
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["SuccessMessage"] = "Вы успешно вышли из системы.";
            return RedirectToAction("Login");
        }

        private string HashPassword(string password)
        {
            byte[] salt = new byte[128 / 8];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            var hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            return $"{Convert.ToBase64String(salt)}.{hashed}";
        }

        private bool VerifyPassword(string inputPassword, string storedPassword)
        {
            var parts = storedPassword.Split('.');
            if (parts.Length != 2)
            {
                return false; // неверный формат хешированного пароля
            }

            var salt = Convert.FromBase64String(parts[0]);
            var hash = parts[1];

            var hashedInput = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: inputPassword,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            return hash == hashedInput;
        }
        [HttpGet]
public IActionResult ChangePassword()
{
    if (!User.Identity.IsAuthenticated)
    {
        return RedirectToAction("Login");
    }

    return View();
}

// POST: /Account/ChangePassword
[HttpPost]
public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
{
    if (!User.Identity.IsAuthenticated)
    {
        return RedirectToAction("Login");
    }

    if (string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
    {
        TempData["ErrorMessage"] = "Все поля должны быть заполнены.";
        return View();
    }
    if (newPassword.Length < 6)
    {
        TempData["ErrorMessage"] = "Пароль слишком короткий. Должно быть минимум 6 символов.";
        return View();
    }

    if (newPassword != confirmPassword)
    {
        TempData["ErrorMessage"] = "Новый пароль и подтверждение пароля не совпадают.";
        return View();
    }

    var email = User.Identity.Name;
    var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == email);

    if (user == null)
    {
        TempData["ErrorMessage"] = "Пользователь не найден.";
        return RedirectToAction("Login");
    }

    if (!VerifyPassword(currentPassword, user.Password))
    {
        TempData["ErrorMessage"] = "Текущий пароль введен неверно.";
        return View();
    }

    // Обновляем пароль
    user.Password = HashPassword(newPassword);
    await _context.SaveChangesAsync();

    TempData["SuccessMessage"] = "Пароль успешно изменен.";
    return RedirectToAction("Profile");
}
// GET: /Account/ChangeEmail
[HttpGet]
public IActionResult ChangeEmail()
{
    if (!User.Identity.IsAuthenticated)
    {
        return RedirectToAction("Login");
    }

    return View();
}

// POST: /Account/ChangeEmail
[HttpPost]
public async Task<IActionResult> ChangeEmail(string currentPassword, string newEmail)
{
    if (!User.Identity.IsAuthenticated)
    {
        return RedirectToAction("Login");
    }

    if (string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newEmail))
    {
        TempData["ErrorMessage"] = "Все поля должны быть заполнены.";
        return View();
    }

    var email = User.Identity.Name;
    var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == email);

    if (user == null)
    {
        TempData["ErrorMessage"] = "Пользователь не найден.";
        return RedirectToAction("Login");
    }

    if (!VerifyPassword(currentPassword, user.Password))
    {
        TempData["ErrorMessage"] = "Неверный пароль.";
        return View();
    }

    if (await _context.Users.AnyAsync(u => u.Email == newEmail))
    {
        TempData["ErrorMessage"] = "Пользователь с таким email уже существует.";
        return View();
    }

    // Обновляем email
    user.Email = newEmail;
    await _context.SaveChangesAsync();

    // Обновляем claims пользователя
    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.Email),
        new Claim("UserId", user.Id.ToString())
    };
    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

    TempData["SuccessMessage"] = "Email успешно изменен.";
    return RedirectToAction("Profile");
}
// GET: /Account/ShippingAddress
[HttpGet]
public async Task<IActionResult> Address()
{
    if (!User.Identity.IsAuthenticated)
    {
        return RedirectToAction("Login");
    }

    var email = User.Identity.Name;
    var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == email);

    if (user == null)
    {
        TempData["ErrorMessage"] = "Пользователь не найден.";
        return RedirectToAction("Login");
    }

    ViewBag.Address = user.Address ?? "Адрес не указан.";
    return View();
}

// POST: /Account/Address
[HttpPost]
public async Task<IActionResult> Address(string Address)
{
    if (!User.Identity.IsAuthenticated)
    {
        return RedirectToAction("Login");
    }

    if (string.IsNullOrEmpty(Address))
    {
        TempData["ErrorMessage"] = "Адрес доставки не может быть пустым.";
        return RedirectToAction("Address");
    }

    var email = User.Identity.Name;
    var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == email);

    if (user == null)
    {
        TempData["ErrorMessage"] = "Пользователь не найден.";
        return RedirectToAction("Login");
    }

    user.Address = Address;
    await _context.SaveChangesAsync();

    TempData["SuccessMessage"] = "Адрес доставки успешно обновлен.";
    return RedirectToAction("Profile");
}


// GET: /Account/Pay
[HttpGet]
public async Task<IActionResult> Pay()
{
    if (!User.Identity.IsAuthenticated)
    {
        return RedirectToAction("Login");
    }

    var email = User.Identity.Name;
    var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == email);

    if (user == null)
    {
        TempData["ErrorMessage"] = "Пользователь не найден.";
        return RedirectToAction("Login");
    }
    if (user.Pay == "Cash")
    {
        ViewBag.Pay = "Наличные";
    }
    else if (user.Pay == "Card" && !string.IsNullOrEmpty(user.CardNumber))
    {
        ViewBag.Pay = $"Банковская карта: {user.CardNumber}";
    }
    else
    {
        ViewBag.Pay = "Не указан";
    }

    // Передаем данные для номера карты, если это нужно
    ViewBag.CardNumber = user.CardNumber ??"";
    return View();
}

// POST: /Account/Pay
[HttpPost]
public async Task<IActionResult> Pay(string pay, string? cardNumber)
{
    if (!User.Identity.IsAuthenticated)
    {
        return RedirectToAction("Login");
    }

    var email = User.Identity.Name;
    var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == email);

    if (user == null)
    {
        TempData["ErrorMessage"] = "Пользователь не найден.";
        return RedirectToAction("Login");
    }

    if (string.IsNullOrEmpty(pay) || (pay == "Card" && string.IsNullOrEmpty(cardNumber)))
    {
        TempData["ErrorMessage"] = "Все поля обязательны для заполнения.";
        return RedirectToAction("Pay");
    }

    user.Pay = pay;

    if (pay == "Card")
    {
        user.CardNumber = cardNumber;
    }
    else
    {
        user.CardNumber = null; // Очищаем номер карты, если выбрана оплата наличными
    }

    await _context.SaveChangesAsync();

    TempData["SuccessMessage"] = "Способ оплаты успешно обновлен.";
    return RedirectToAction("Profile");
}


    }
    
}
