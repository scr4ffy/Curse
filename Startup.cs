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
        // ��������� ����������� � ���� ������
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));

        services.AddDbContext<BikeShopContext>(options =>
            options.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));

        services.AddControllersWithViews();
        
        services.AddScoped<WishlistService>();

        // ����������� IHttpContextAccessor
        services.AddHttpContextAccessor();

        // ����������� CartService
        services.AddScoped<CartService>();
        

        // ���������� �������������� � �������������� ����
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/Account/Login"; // ���� � �������� �����
                options.LogoutPath = "/Account/Logout"; // ���� � �������� ������
                options.AccessDeniedPath = "/Account/AccessDenied"; // ���� ��� ������ � ������� (�� �������)
            });

        // ���������� �������� ����������� (�� �������)
        services.AddAuthorization();

        // ���������� ������
        services.AddSession();

        // ���������� ������������ � ���������������
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
        // ���������� ������ � �������� ��������
        app.UseSession();

        // ���������� �������������� � ����������� � �������� ��������
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
