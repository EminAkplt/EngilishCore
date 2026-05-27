using EnglishCore.Models;
using Microsoft.EntityFrameworkCore;

namespace EnglishCore.Data
{
    // Uygulama açılırken çağrılır. Eğer hiç Admin kullanıcı yoksa default bir admin hesabı oluşturur.
    // Dev ortamında ilk kurulumda manuel kullanıcı oluşturmaktan kurtarır.
    public static class SeedData
    {
        // Program.cs'ten çağrılır, app.Run()'dan önce
        public static async Task SeedAsync(ApplicationDbContext db)
        {
            // Bekleyen migration varsa otomatik uygula (dev kolaylığı)
            await db.Database.MigrateAsync();

            // Sistemde Admin rolünde en az bir kullanıcı var mı kontrol et
            var hasAdmin = await db.AppUsers.AnyAsync(u => u.Role == "Admin");
            if (hasAdmin)
            {
                // Zaten admin var, seed işlemine gerek yok
                return;
            }

            // Default admin hesabı — username: admin, password: admin (şimdilik düz metin, sonra BCrypt)
            db.AppUsers.Add(new AppUser
            {
                Username = "admin",
                PasswordHash = "admin",                  // ŞİMDİLİK DÜZ METİN — production öncesi hash'lenecek
                Role = "Admin",
                CurrentLevel = "A1",                     // Admin için seviye önemsiz, default A1 koyuyoruz
                HighestReachedLevel = "A1",
                DailyWordLimit = 10,
                DailySentenceLimit = 5
            });

            await db.SaveChangesAsync();
        }
    }
}
