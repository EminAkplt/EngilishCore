namespace EnglishCore.Models
{
    // Sisteme giriş yapacak olan kişilerin (öğrenci veya yönetici) kayıtlarını tutan ana tablo
    public class AppUser
    {
        // Veri tabanında her kullanıcının benzersiz kimlik numarası (Primary Key)
        public int Id { get; set; }

        // Kullanıcının sisteme girerken kullanacağı kullanıcı adı
        public string Username { get; set; }

        // Kullanıcının sisteme girerken kullanacağı şifresi
        public string Password { get; set; }

        // Kişinin yetkisini belirler. "Admin" kelime ekler, "User" sistemi kullanır
        public string Role { get; set; }

        // Kullanıcının havuzdan hangi zorlukta veri çekeceğini belirler (Örn: "A1", "B2")
        public string CurrentLevel { get; set; }

        // Kullanıcının her gün karşısına çıkmasını istediği maksimum kelime sayısı
        public int DailyWordLimit { get; set; } = 10;

        // Kullanıcının her gün karşısına çıkmasını istediği maksimum cümle sayısı
        public int DailySentenceLimit { get; set; } = 5;
    }
}