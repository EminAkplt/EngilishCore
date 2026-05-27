using EnglishCore.Data;
using EnglishCore.Models;
using EnglishCore.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EngilishCore.Controllers
{
    // Kullanıcı kayıt, giriş ve çıkış işlemlerini yöneten controller
    public class AccountController : Controller
    {
        // DbContext'i DI ile alıyoruz - constructor injection
        private readonly ApplicationDbContext _db;

        public AccountController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: /Account/Register
        // Boş kayıt formunu render eder.
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        // Formdan gelen veriyi alır, doğrular, AppUser oluşturur, DB'ye yazar ve otomatik giriş yapar.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel vm)
        {
            // 1. Validation - attribute'lar zaten ModelState'e işlemiş durumda
            if (!ModelState.IsValid)
                return View(vm);

            // 2. Username unique kontrol (DB seviyesinde de var ama önce nazik bir hata mesajı verelim)
            var exists = await _db.AppUsers.AnyAsync(u => u.Username == vm.Username);
            if (exists)
            {
                ModelState.AddModelError(nameof(vm.Username), "Bu kullanıcı adı zaten alınmış.");
                return View(vm);
            }

            // 3. ViewModel -> Entity dönüşümü (mapping)
            var user = new AppUser
            {
                Username = vm.Username,
                PasswordHash = vm.Password,            // şimdilik düz metin, sonra BCrypt
                Role = "User",                          // güvenlik: kullanıcı kendi rolünü seçemez
                CurrentLevel = vm.SelectedLevel,
                HighestReachedLevel = vm.SelectedLevel  // ilk kayıtta ikisi eşit (Plan.md 4.4)
            };

            // 4. DB'ye yaz
            _db.AppUsers.Add(user);
            await _db.SaveChangesAsync();

            // 5. Otomatik giriş yap - cookie oluştur
            await SignInUserAsync(user);

            // 6. Ana sayfaya yönlendir
            return RedirectToAction("Index", "Home");
        }

        // Cookie ile kullanıcıyı giriş yaptıran yardımcı method (ileride Login action'ı da kullanacak)
        private async Task SignInUserAsync(AppUser user)
        {
            // Claim'ler kullanıcı hakkındaki "kanıtlar" - cookie içine şifreli yazılırlar
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // User.Identity.Name vs için
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)                      // [Authorize(Roles="Admin")] için
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        }
    }
}
