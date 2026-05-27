using EnglishCore.Data;
using EnglishCore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EngilishCore.Controllers
{
    // Admin'in Word (kelime) CRUD işlemlerini yönettiği controller. Sadece Admin rolü erişebilir.
    [Authorize(Roles = "Admin")]
    public class WordsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public WordsController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: /Words — kelimeleri listeler, opsiyonel seviye filtresi
        public async Task<IActionResult> Index(string? level = null)
        {
            ViewData["SelectedLevel"] = level;

            var query = _db.Words.AsQueryable();
            if (!string.IsNullOrEmpty(level))
                query = query.Where(w => w.Level == level);

            var words = await query
                .OrderBy(w => w.Level)
                .ThenBy(w => w.EnglishText)
                .ToListAsync();

            return View(words);
        }

        // GET: /Words/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Words/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Word word)
        {
            if (!ModelState.IsValid)
                return View(word);

            _db.Words.Add(word);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: /Words/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var word = await _db.Words.FindAsync(id);
            if (word == null) return NotFound();
            return View(word);
        }

        // POST: /Words/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Word word)
        {
            if (id != word.Id) return BadRequest();

            if (!ModelState.IsValid)
                return View(word);

            _db.Words.Update(word);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: /Words/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var word = await _db.Words.FindAsync(id);
            if (word == null) return NotFound();
            return View(word);
        }

        // POST: /Words/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var word = await _db.Words.FindAsync(id);
            if (word != null)
            {
                _db.Words.Remove(word);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
