using EnglishCore.Data;
using EnglishCore.Models;
using EnglishCore.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EngilishCore.Controllers
{
    // Kullanıcı kayıt, giriş ve çıkış işlemlerini yöneten controller
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AccountController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: /Account/Register — boş kayıt formunu render eder
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register — formu alır, doğrular, AppUser oluşturur ve otomatik giriş yapar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var exists = await _db.AppUsers.AnyAsync(u => u.Username == vm.Username);
            if (exists)
            {
                ModelState.AddModelError(nameof(vm.Username), "Bu kullanıcı adı zaten alınmış.");
                return View(vm);
            }

            var user = new AppUser
            {
                Username = vm.Username,
                PasswordHash = vm.Password,                 // şimdilik düz metin, sonra BCrypt
                Role = "User",                               // güvenlik: kullanıcı kendi rolünü seçemez
                CurrentLevel = vm.SelectedLevel,
                HighestReachedLevel = vm.SelectedLevel       // ilk kayıtta ikisi eşit (Plan.md 4.4)
            };

            _db.AppUsers.Add(user);
            await _db.SaveChangesAsync();

            await SignInUserAsync(user);

            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Login — boş giriş formunu render eder
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Login — kullanıcıyı doğrular ve cookie oluşturur
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel vm, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
                return View(vm);

            var user = await _db.AppUsers.FirstOrDefaultAsync(u => u.Username == vm.Username);

            // Aynı mesaj — username'in var/yok olduğunu sızdırmıyoruz
            if (user == null || user.PasswordHash != vm.Password)
            {
                ModelState.AddModelError(string.Empty, "Kullanıcı adı veya şifre hatalı.");
                return View(vm);
            }

            await SignInUserAsync(user);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        // POST: /Account/Logout — cookie'yi siler
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/AccessDenied — yetkisiz erişim sayfası
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // Claim'leri hazırlayıp cookie ile sign-in yapan yardımcı method
        private async Task SignInUserAsync(AppUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        }
    }
}
