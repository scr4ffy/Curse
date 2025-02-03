using BikeShop.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Globalization;
public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        // Настройка подключения к базе данных
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));

        services.AddDbContext<BikeShopContext>(options =>
            options.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));

        services.AddControllersWithViews();
        
        services.AddScoped<WishlistService>();

        // Регистрация IHttpContextAccessor
        services.AddHttpContextAccessor();

        // Регистрация CartService
        services.AddScoped<CartService>();
        

        // Добавление аутентификации с использованием куки
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/Account/Login"; // Путь к странице входа
                options.LogoutPath = "/Account/Logout"; // Путь к странице выхода
                options.AccessDeniedPath = "/Account/AccessDenied"; // Путь при отказе в доступе (по желанию)
            });

        // Добавление политики авторизации (по желанию)
        services.AddAuthorization();

        // Добавление сессий
        services.AddSession();

        // Добавление контроллеров с представлениями
        services.AddControllersWithViews();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();
        app.UseStaticFiles();
        // Добавление сессии в конвейер запросов
        app.UseSession();

        // Добавление аутентификации и авторизации в конвейер запросов
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
        });
    }
}
