using System;

namespace EnglishCore.Models
{
    // Hangi kullanıcının, hangi cümleyi kaç kere gördüğünü takip eden köprü tablosu
    public class UserSentenceProgress
    {
        // Bu ilerleme kaydının benzersiz kimlik numarası (Primary Key)
        public int Id { get; set; }

        // Bu ilerlemenin hangi kullanıcıya ait olduğunu belirten numara (Foreign Key)
        public int AppUserId { get; set; }

        // Bu ilerlemenin hangi cümle için tutulduğunu belirten numara (Foreign Key)
        public int SentenceId { get; set; }

        // 0: Yeni cümle, 1: Tekrar bekleyen cümle, 2: Öğrenilenler Havuzuna düşmüş cümle
        public int ViewCount { get; set; } = 0;

        // Sistemin gün atlamalarında nerede kaldığını bulmasını sağlayan son görülme tarihi (UTC)
        public DateTime? LastReviewed { get; set; }

        // Navigation property: bu kaydın hangi kullanıcıya ait olduğunu sorgularda Include ile getirir
        public AppUser AppUser { get; set; } = null!;

        // Navigation property: bu kaydın hangi cümleye ait olduğunu sorgularda Include ile getirir
        public Sentence Sentence { get; set; } = null!;
    }
}
