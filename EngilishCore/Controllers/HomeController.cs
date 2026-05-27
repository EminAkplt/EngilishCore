using EnglishCore.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace EngilishCore.Controllers
{
    // Ana sayfa (landing) ve global hata sayfasını yöneten controller
    public class HomeController : Controller
    {
        // GET: / veya /Home/Index — landing/ana sayfa
        // Auth durumuna göre view içinde dinamik içerik gösterilir
        public IActionResult Index()
        {
            return View();
        }

        // GET: /Home/Error — production'da exception olunca buraya yönlendirilir
        // [ResponseCache] hata sayfasının cache'lenmesini engeller (her seferinde taze RequestId görelim)
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
