using System;

namespace EnglishCore.Models
{
    // Hangi kullanıcının, hangi kelimeyi kaç kere gördüğünü takip eden köprü tablosu
    public class UserWordProgress
    {
        // Bu ilerleme kaydının benzersiz kimlik numarası (Primary Key)
        public int Id { get; set; }

        // Bu ilerlemenin hangi kullanıcıya ait olduğunu belirten numara (Foreign Key)
        public int AppUserId { get; set; }

        // Bu ilerlemenin hangi kelime için tutulduğunu belirten numara (Foreign Key)
        public int WordId { get; set; }

        // 0: Hiç görülmedi, 1: Dün görüldü (Tekrarda), 2: İki kez bilindi (Öğrenilenler Havuzunda)
        public int ViewCount { get; set; } = 0;

        // Kullanıcı gün atladığında takvimin kaymaması için kelimeye son bakılan tarih
        public DateTime? LastReviewed { get; set; }
    }
}