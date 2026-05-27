using EnglishCore.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EngilishCore.Controllers
{
    // Kullanıcının seviyesindeki kelimeleri kart olarak gösteren controller.
    // İlk versiyon — sadece kelime listeleme. Daily limit / Pool / 2-aşamalı tekrar mantığı sonraki adım.
    [Authorize]
    public class StudyController : Controller
    {
        // DbContext'i DI üzerinden alıyoruz — kelimeleri ve kullanıcı bilgisini DB'den çekmek için
        private readonly ApplicationDbContext _db;

        public StudyController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: /Study — kullanıcının CurrentLevel'indeki kelimeleri kart akışı olarak gösterir
        public async Task<IActionResult> Index()
        {
            // Cookie'deki claim'den kullanıcı ID'sini al (Login sırasında ClaimTypes.NameIdentifier set edilmişti)
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out var userId))
            {
                // Claim hatalı veya yok — auth sorununu işaret eder, login'e gönder
                return Forbid();
            }

            // Kullanıcıyı DB'den çek — CurrentLevel ve DailyWordLimit'i okumak için
            var user = await _db.AppUsers.FindAsync(userId);
            if (user == null) return Forbid();

            // Kullanıcının seviyesindeki kelimeleri al
            // ŞİMDİLİK: sadece Level filtresi + Id sıralı + DailyWordLimit kadar
            // Plan.md 4.2: Yarın "Yeni + Tekrar" mantığı, ViewCount güncellemesi ve havuz buraya entegre edilecek
            var words = await _db.Words
                .Where(w => w.Level == user.CurrentLevel)
                .OrderBy(w => w.Id)
                .Take(user.DailyWordLimit)
                .ToListAsync();

            // View'da seviye rozetini göstermek için ViewData ile aktarıyoruz
            ViewData["UserLevel"] = user.CurrentLevel;

            return View(words);
        }
    }
}
