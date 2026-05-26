using System.ComponentModel.DataAnnotations;

namespace EnglishCore.Models
{
    // Sisteme giriş yapacak olan kişilerin (öğrenci veya yönetici) kayıtlarını tutan ana tablo
    public class AppUser
    {
        // Veri tabanında her kullanıcının benzersiz kimlik numarası (Primary Key)
        public int Id { get; set; }

        // Kullanıcının sisteme girerken kullanacağı kullanıcı adı
        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        // Kullanıcının şifresinin hash'lenmiş hali. Düz metin şifre asla burada tutulmaz.
        [Required]
        [MaxLength(200)]
        public string PasswordHash { get; set; } = string.Empty;

        // Kişinin yetkisini belirler. "Admin" kelime ekler, "User" sistemi kullanır
        [Required]
        [MaxLength(10)]
        public string Role { get; set; } = string.Empty;

        // Kullanıcının havuzdan hangi zorlukta veri çekeceğini belirler (Örn: "A1", "B2")
        [Required]
        [MaxLength(2)]
        public string CurrentLevel { get; set; } = string.Empty;

        // Kullanıcının otomatik geçişle ulaştığı en üst seviye. Manuel geri dönüşte değişmez.
        [Required]
        [MaxLength(2)]
        public string HighestReachedLevel { get; set; } = string.Empty;

        // Kullanıcının her gün karşısına çıkmasını istediği maksimum kelime sayısı
        [Range(1, 50)]
        public int DailyWordLimit { get; set; } = 10;

        // Kullanıcının her gün karşısına çıkmasını istediği maksimum cümle sayısı
        [Range(1, 25)]
        public int DailySentenceLimit { get; set; } = 5;
    }
}
