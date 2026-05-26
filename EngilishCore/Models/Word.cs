using System.ComponentModel.DataAnnotations;

namespace EnglishCore.Models
{
    // Sistemdeki tüm İngilizce kelimelerin saf halde tutulduğu sabit sözlük tablosu
    public class Word
    {
        // Her kelimenin veri tabanındaki benzersiz kimlik numarası (Primary Key)
        public int Id { get; set; }

        // Kelimenin İngilizce orijinal yazılışı (Örn: "Apple")
        [Required]
        [MaxLength(100)]
        public string EnglishText { get; set; } = string.Empty;

        // Kelimenin Türkçe tam karşılığı (Örn: "Elma")
        [Required]
        [MaxLength(200)]
        public string TurkishText { get; set; } = string.Empty;

        // Bu kelimenin hangi zorluk derecesine ait olduğu (Örn: "A1")
        [Required]
        [MaxLength(2)]
        public string Level { get; set; } = string.Empty;
    }
}
