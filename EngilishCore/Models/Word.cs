namespace EnglishCore.Models
{
    // Sistemdeki tüm İngilizce kelimelerin saf halde tutulduğu sabit sözlük tablosu
    public class Word
    {
        // Her kelimenin veri tabanındaki benzersiz kimlik numarası (Primary Key)
        public int Id { get; set; }

        // Kelimenin İngilizce orijinal yazılışı (Örn: "Apple")
        public string EnglishText { get; set; }

        // Kelimenin Türkçe tam karşılığı (Örn: "Elma")
        public string TurkishText { get; set; }

        // Bu kelimenin hangi zorluk derecesine ait olduğu (Örn: "A1")
        public string Level { get; set; }
    }
}