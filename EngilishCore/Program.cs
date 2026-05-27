using EnglishCore.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace EngilishCore
{
    public class Program
    {
        // Main async oldu çünkü uygulama açılırken Seed asenkron çalışıyor (DB'ye yazıyor)
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // MVC servislerini ekle (Controller + View desteği)
            builder.Services.AddControllersWithViews();

            // EF Core + PostgreSQL DbContext'i DI konteynerine kaydet
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Cookie tabanlı kimlik doğrulama (Login/Logout)
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";              // Giriş yapmamış kullanıcı buraya yönlendirilir
                    options.AccessDeniedPath = "/Account/AccessDenied"; // Yetkisi olmayan buraya yönlendirilir
                    options.ExpireTimeSpan = TimeSpan.FromDays(7);     // Cookie 7 gün geçerli
                    options.SlidingExpiration = true;                  // Aktif kullanımda süre kendiliğinden uzar
                });

            // [Authorize] ve [Authorize(Roles = "Admin")] attribute'ları için
            builder.Services.AddAuthorization();

            var app = builder.Build();

            // Seed: Uygulama açılırken Admin yoksa default bir admin oluştur (dev kolaylığı)
            // Scope kullanıyoruz çünkü ApplicationDbContext "scoped" yaşam döngüsünde
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await SeedData.SeedAsync(db);
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();   // Önce "kim?" (cookie'den kullanıcı kimliği çıkarılır)
            app.UseAuthorization();    // Sonra "ne yapabilir?" (rol/yetki kontrolü)

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            await app.RunAsync();
        }
    }
}
