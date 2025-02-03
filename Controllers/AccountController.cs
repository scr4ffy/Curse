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
        // �������� �� ������������ email ����� ���������� ��������� � ������ ����� ������� � ������ ���������� �������
        var emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.(ru|com|net)$";
        if (!System.Text.RegularExpressions.Regex.IsMatch(model.Email, emailPattern))
        {
            ModelState.AddModelError("Email", "Email ������ ���� � ������� @(�����).ru, .com ��� .net � ��������� ������ ��������� �������.");
            TempData["ErrorMessage"] = "������������ ������ Email ��� ������������ �� ��������� �������.";
            return View(model);
        }
                if (model.Password.Length < 6)
        {
            ModelState.AddModelError("Password", "������ ������ ��������� ������� 6 ��������.");
            TempData["ErrorMessage"] = "������ ������� ��������. ������ ���� ������� 6 ��������.";
            return View(model);
        }

        if (await _context.Users.AnyAsync(u => u.Email == model.Email))
        {
            ModelState.AddModelError(string.Empty, "������������ � ����� email ��� ����������.");
            TempData["ErrorMessage"] = "������������ � ����� email ��� ����������.";
            return View(model);
        
        }

        var user = new User
        {
            Email = model.Email,
            Password = HashPassword(model.Password)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "����������� ������ �������. ����������, ������� � �������.";
        return RedirectToAction("Login");
    }

    TempData["ErrorMessage"] = "������ �����������. ����������, ��������� ��������� ������.";
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
                // ��������� ���� ��������������
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Email),
                    new Claim("UserId", user.Id.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                TempData["SuccessMessage"] = "�� ������� ����� � �������.";
                return RedirectToAction("Profile");
            }

            ModelState.AddModelError(string.Empty, "�������� email ��� ������.");
            TempData["ErrorMessage"] = "������ �����. ����������, ��������� ��������� ������.";
            return View();
        }

        // GET: /Account/Profile
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            // ��������, ��� User.Identity �� null � �������� �������������������
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login");
            }

            var email = User.Identity.Name; // ������ �� �������, ��� ��� �� null
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == email);
            
            if (user == null)
            {
                TempData["ErrorMessage"] = "������������ �� ������.";
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
                var userIdString = User.FindFirstValue("UserId"); // �������� ID ������������ �� claims
                if (int.TryParse(userIdString, out int userId)) // ����������� ������ � int
                {
                    var user = await _context.Users.FindAsync(userId);

                    if (user != null)
                    {
                        // ��������� ����������� ����� �����
                        var fileName = $"{userId}_{Path.GetFileName(file.FileName)}";
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                        // ���������� ����� � ����� wwwroot/images
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        // ���������� ���� � ����� � ������� ������������
                        user.ProfileImagePath = fileName;
                        await _context.SaveChangesAsync();

                        TempData["SuccessMessage"] = "���� ������� ������� ���������.";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "������������ �� ������.";
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "������ ��� �������������� ID ������������.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "������ �������� ����.";
            }

            return RedirectToAction("Profile"); // ��������������� �� ������������� �������
        }

        // POST: /Account/Logout
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["SuccessMessage"] = "�� ������� ����� �� �������.";
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
                return false; // �������� ������ ������������� ������
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
        TempData["ErrorMessage"] = "��� ���� ������ ���� ���������.";
        return View();
    }
    if (newPassword.Length < 6)
    {
        TempData["ErrorMessage"] = "������ ������� ��������. ������ ���� ������� 6 ��������.";
        return View();
    }

    if (newPassword != confirmPassword)
    {
        TempData["ErrorMessage"] = "����� ������ � ������������� ������ �� ���������.";
        return View();
    }

    var email = User.Identity.Name;
    var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == email);

    if (user == null)
    {
        TempData["ErrorMessage"] = "������������ �� ������.";
        return RedirectToAction("Login");
    }

    if (!VerifyPassword(currentPassword, user.Password))
    {
        TempData["ErrorMessage"] = "������� ������ ������ �������.";
        return View();
    }

    // ��������� ������
    user.Password = HashPassword(newPassword);
    await _context.SaveChangesAsync();

    TempData["SuccessMessage"] = "������ ������� �������.";
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
        TempData["ErrorMessage"] = "��� ���� ������ ���� ���������.";
        return View();
    }

    var email = User.Identity.Name;
    var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == email);

    if (user == null)
    {
        TempData["ErrorMessage"] = "������������ �� ������.";
        return RedirectToAction("Login");
    }

    if (!VerifyPassword(currentPassword, user.Password))
    {
        TempData["ErrorMessage"] = "�������� ������.";
        return View();
    }

    if (await _context.Users.AnyAsync(u => u.Email == newEmail))
    {
        TempData["ErrorMessage"] = "������������ � ����� email ��� ����������.";
        return View();
    }

    // ��������� email
    user.Email = newEmail;
    await _context.SaveChangesAsync();

    // ��������� claims ������������
    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.Email),
        new Claim("UserId", user.Id.ToString())
    };
    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

    TempData["SuccessMessage"] = "Email ������� �������.";
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
        TempData["ErrorMessage"] = "������������ �� ������.";
        return RedirectToAction("Login");
    }

    ViewBag.Address = user.Address ?? "����� �� ������.";
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
        TempData["ErrorMessage"] = "����� �������� �� ����� ���� ������.";
        return RedirectToAction("Address");
    }

    var email = User.Identity.Name;
    var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == email);

    if (user == null)
    {
        TempData["ErrorMessage"] = "������������ �� ������.";
        return RedirectToAction("Login");
    }

    user.Address = Address;
    await _context.SaveChangesAsync();

    TempData["SuccessMessage"] = "����� �������� ������� ��������.";
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
        TempData["ErrorMessage"] = "������������ �� ������.";
        return RedirectToAction("Login");
    }
    if (user.Pay == "Cash")
    {
        ViewBag.Pay = "��������";
    }
    else if (user.Pay == "Card" && !string.IsNullOrEmpty(user.CardNumber))
    {
        ViewBag.Pay = $"���������� �����: {user.CardNumber}";
    }
    else
    {
        ViewBag.Pay = "�� ������";
    }

    // �������� ������ ��� ������ �����, ���� ��� �����
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
        TempData["ErrorMessage"] = "������������ �� ������.";
        return RedirectToAction("Login");
    }

    if (string.IsNullOrEmpty(pay) || (pay == "Card" && string.IsNullOrEmpty(cardNumber)))
    {
        TempData["ErrorMessage"] = "��� ���� ����������� ��� ����������.";
        return RedirectToAction("Pay");
    }

    user.Pay = pay;

    if (pay == "Card")
    {
        user.CardNumber = cardNumber;
    }
    else
    {
        user.CardNumber = null; // ������� ����� �����, ���� ������� ������ ���������
    }

    await _context.SaveChangesAsync();

    TempData["SuccessMessage"] = "������ ������ ������� ��������.";
    return RedirectToAction("Profile");
}


    }
    
}
