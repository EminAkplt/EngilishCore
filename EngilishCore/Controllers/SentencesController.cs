using EnglishCore.Data;
using EnglishCore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EngilishCore.Controllers
{
    // Admin'in Sentence (cümle) CRUD işlemlerini yönettiği controller. Sadece Admin rolü erişebilir.
    [Authorize(Roles = "Admin")]
    public class SentencesController : Controller
    {
        // DbContext'i DI üzerinden alıyoruz - veritabanı işlemleri için
        private readonly ApplicationDbContext _db;

        public SentencesController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: /Sentences — cümleleri listeler, opsiyonel seviye filtresi (?level=A1)
        public async Task<IActionResult> Index(string? level = null)
        {
            // Dropdown'da seçili seviyeyi göstermek için view'a aktarıyoruz
            ViewData["SelectedLevel"] = level;

            // IQueryable üzerinde koşullu filtreleme
            var query = _db.Sentences.AsQueryable();
            if (!string.IsNullOrEmpty(level))
                query = query.Where(s => s.Level == level);

            // Seviye sonra İngilizce metne göre sıralı listele
            var sentences = await query
                .OrderBy(s => s.Level)
                .ThenBy(s => s.EnglishText)
                .ToListAsync();

            return View(sentences);
        }

        // GET: /Sentences/Create — yeni cümle ekleme formunu render eder
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Sentences/Create — formdan gelen cümleyi DB'ye kaydeder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Sentence sentence)
        {
            // Attribute validation'larının sonucu (Required, MaxLength vb.)
            if (!ModelState.IsValid)
                return View(sentence);

            _db.Sentences.Add(sentence);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: /Sentences/Edit/5 — düzenleme formunu mevcut değerlerle doldurarak gösterir
        public async Task<IActionResult> Edit(int id)
        {
            var sentence = await _db.Sentences.FindAsync(id);
            if (sentence == null) return NotFound();
            return View(sentence);
        }

        // POST: /Sentences/Edit/5 — düzenlemeleri DB'ye yazar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Sentence sentence)
        {
            // URL'deki id ile form'dan gelen Id uyumsuzsa kötü niyetli istek — reddet
            if (id != sentence.Id) return BadRequest();

            if (!ModelState.IsValid)
                return View(sentence);

            _db.Sentences.Update(sentence);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: /Sentences/Delete/5 — silme onay sayfası (kullanıcıya neyi sileceğini gösterir)
        public async Task<IActionResult> Delete(int id)
        {
            var sentence = await _db.Sentences.FindAsync(id);
            if (sentence == null) return NotFound();
            return View(sentence);
        }

        // POST: /Sentences/Delete/5 — kullanıcı silmeyi onayladığında çalışır
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sentence = await _db.Sentences.FindAsync(id);
            if (sentence != null)
            {
                _db.Sentences.Remove(sentence);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
