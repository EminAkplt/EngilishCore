namespace EnglishCore.Models
{
    // Sistemdeki tüm İngilizce cümlelerin saf halde tutulduğu sabit sözlük tablosu
    public class Sentence
    {
        // Her cümlenin veri tabanındaki benzersiz kimlik numarası (Primary Key)
        public int Id { get; set; }

        // Cümlenin İngilizce orijinal yazılışı (Örn: "This is a book.")
        public string EnglishText { get; set; }

        // Cümlenin Türkçe tam karşılığı (Örn: "Bu bir kitaptır.")
        public string TurkishText { get; set; }

        // Bu cümlenin hangi zorluk derecesine ait olduğu (Örn: "A1")
        public string Level { get; set; }
    }
}