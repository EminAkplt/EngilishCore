using System.ComponentModel.DataAnnotations;

namespace EnglishCore.Models
{
    // Sistemdeki tüm İngilizce cümlelerin saf halde tutulduğu sabit sözlük tablosu
    public class Sentence
    {
        // Her cümlenin veri tabanındaki benzersiz kimlik numarası (Primary Key)
        public int Id { get; set; }

        // Cümlenin İngilizce orijinal yazılışı (Örn: "This is a book.")
        [Required]
        [MaxLength(300)]
        public string EnglishText { get; set; } = string.Empty;

        // Cümlenin Türkçe tam karşılığı (Örn: "Bu bir kitaptır.")
        [Required]
        [MaxLength(500)]
        public string TurkishText { get; set; } = string.Empty;

        // Bu cümlenin hangi zorluk derecesine ait olduğu (Örn: "A1")
        [Required]
        [MaxLength(2)]
        public string Level { get; set; } = string.Empty;
    }
}
