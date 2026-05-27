# EngilishCore – Proje Planı

Kümülatif İngilizce Öğrenme Platformu. Bu doküman projenin uçtan uca tasarım ve teknik kararlarını kapsar. Kod yazımı bu dosyada belirtilen kurallara birebir uyacaktır.

---

## 1. Genel Mimari ve Teknolojiler

- **Framework:** .NET 8, ASP.NET Core MVC (tek proje, Server-Side Rendering)
- **Dil:** C# 12
- **Veritabanı:** PostgreSQL
- **ORM:** Entity Framework Core 8 – Code-First yaklaşımı
- **Provider:** `Npgsql.EntityFrameworkCore.PostgreSQL`
- **Mimari yaklaşım:** Geleneksel MVC. API ayrımı, mikroservis, ayrı katmanlı proje bölünmesi **yapılmaz**. Veriler doğrudan Controller'da işlenip View'e gönderilir.
- **Çözüm yapısı:** Tek proje (`EngilishCore`). Klasör organizasyonu:
  - `Models/` – Entity sınıfları
  - `Data/` – `ApplicationDbContext`
  - `Controllers/` – MVC controller'lar
  - `Views/` – Razor view'lar
  - `Migrations/` – EF Core migration dosyaları
  - `wwwroot/` – Statik dosyalar
- **Namespace standardı:**
  - Proje genel: `EngilishCore`
  - Modeller: `EnglishCore.Models`
  - DbContext: `EnglishCore.Data`

---

## 2. Rol ve Yetkilendirme

Sistemde iki rol vardır. Her ikisi de aynı `AppUser` tablosunda tutulur, ayrım `Role` kolonundan yapılır.

| Rol | Yetkiler |
|---|---|
| **Admin** | Word ve Sentence tablolarında seviye bazlı (A1, A2, B1, B2, C1, C2) CRUD işlemi yapar. Sisteme kelime/cümle girer. |
| **User** | Üye olur, günlük limitlerini belirler, kart döngüsünü ilerletir, havuzdan içerik yönetir. |

Admin paneline erişim `[Authorize(Roles = "Admin")]` ile kısıtlanır. Kullanıcı sayfalarına `[Authorize]` yeterlidir.

---

## 3. Günlük Hedef Limitleri

Her kullanıcı kendi günlük çalışma kontenjanını profilinden belirler. Sınırlar:

| İçerik | Minimum | Maksimum |
|---|---|---|
| Kelime | 1 | 50 |
| Cümle | 1 | 25 |

Bu limitler hem model üzerinde `[Range]` attribute'u ile, hem de UI tarafında form validation ile zorlanır.

---

## 4. Kümülatif Öğrenme Akışı

### 4.1 Kaldığı Yerden Devam Kuralı

Akış takvime değil, kullanıcının kendi ilerlemesine bağlıdır.

- Atlanan günler için sistem **geriye sarmaz, sıfırlamaz, ceza uygulamaz**.
- Kullanıcı tekrar giriş yaptığında, en son `ViewCount` ve `LastReviewed` değerlerinden devam eder.
- "Yeni kelime/cümle" seçimi: `UserWordProgress` (veya `UserSentenceProgress`) tablosunda **bu kullanıcı için kaydı olmayan** ve seviyesi `AppUser.CurrentLevel` ile eşleşen Word/Sentence kayıtlarından, `Id` sırasına göre çekilir.

### 4.2 2-Aşamalı Tekrar Döngüsü

Her içerik kullanıcının önünden tam olarak iki kez geçer.

| Aşama | `ViewCount` | Anlam |
|---|---|---|
| 0 | 0 (kayıt yok) | Henüz görülmedi – "yeni" havuzunda |
| 1 | 1 | İlk kez görüldü – ertesi gün tekrar olarak çıkar |
| 2 | 2 | İkinci kez görüldü – Öğrenilenler Havuzu'na taşındı |

Kullanıcı bir günde `DailyWordLimit` kadar yeni + bir önceki günden kalan tekrar kelimelerini görür. Cümleler için aynı kural geçerlidir.

**Tekrar listesinin seçim koşulu:** `ViewCount == 1 AND LastReviewed < bugün (UTC)`.

### 4.3 Öğrenilenler Havuzu ve Geri Al

- `ViewCount == 2` olan tüm kayıtlar Öğrenilenler Havuzu'nu oluşturur.
- Bu kayıtlar artık günlük döngüye otomatik dahil edilmez.
- Kullanıcı, havuz ekranındaki **Geri Al** butonu ile bir kaydı aktif döngüye iade edebilir. Geri alma işlemi şunu yapar:
  - `ViewCount = 0`
  - `LastReviewed = null`
- Sonuç: Kelime/cümle "hiç görülmemiş" statüsüne döner ve sıradaki yeni içerik kontenjanından döngüye girer.
- "Kalıcı arşiv" mantığı yoktur. Kullanıcı havuzdaki her şeyi geri alabilir.

### 4.4 Seviye Geçiş Mantığı

Seviye sırası sabittir: **A1 → A2 → B1 → B2 → C1 → C2**.

- Kullanıcı kayıt sırasında bir seviye seçer. Bu değer hem `AppUser.CurrentLevel` hem `AppUser.HighestReachedLevel` alanlarına yazılır.
- Günlük kart akışı **sadece** `CurrentLevel` ile eşleşen Word/Sentence kayıtlarından çekilir. Başka seviyenin içeriği günlük akışta görünmez.
- **Otomatik geçiş:** Bir kelime veya cümle havuza taşındığı anda (`ViewCount` 1 → 2), sistem mevcut seviyenin tüm Word **ve** Sentence kayıtlarının havuza düşüp düşmediğini kontrol eder. Hepsi düştüyse **ve** `CurrentLevel == HighestReachedLevel` ise, her iki alan birden bir sonraki seviyeye yükseltilir.
- **Manuel geri dönüş:** Kullanıcı profil ekranından eski bir seviyeye dönebilir. Bu işlem **sadece** `CurrentLevel`'i değiştirir; `HighestReachedLevel` aynı kalır.
- Bu sayede kullanıcı A1'e geri döndükten sonra Geri Al + tekrar tamamlama yapsa bile sistem onu otomatik A2'ye atmaz (çünkü `CurrentLevel < HighestReachedLevel` ve eşitlik koşulu bozulmuş olur).
- C2 seviyesinde otomatik geçiş tetiklenmez (en üst seviye, sıradaki yok).

---

## 5. İlerleme Çizelgesi (Progress Matrix)

A1, A2, B1, B2, C1, C2 seviyelerinin **her biri** için ayrı ayrı kelime ve cümle ilerleme yüzdesi UI üzerinde gösterilir.

```
İlerleme Yüzdesi (%) = ( Havuzdaki Öğrenilen İçerik Sayısı / Seviyedeki Toplam İçerik Sayısı ) × 100
```

- "Havuzdaki Öğrenilen İçerik" = `UserWordProgress` (veya `UserSentenceProgress`) tablosunda `ViewCount == 2` olan ve ilgili seviyeye ait kayıtlar.
- "Seviyedeki Toplam İçerik" = `Word` (veya `Sentence`) tablosunda ilgili `Level` için toplam kayıt sayısı.
- Yüzde **runtime'da hesaplanır**, tabloya ayrı bir alan olarak yazılmaz.
- Geri Al sonrası yüzde otomatik düşer; yeni içerik öğrenildikçe yükselir.

---

## 6. Modüller ve Ekranlar

### 6.1 Günlük Öğrenme Kartları
- Tinder-card tarzı kart akışı.
- Liste: (Yeni içerik) + (Dünden kalan tekrar içeriği).
- Kart ön yüzü: İngilizce metin + dış görsel API'sinden anlık çekilen resim.
- Kart çevrildiğinde: Türkçe karşılık.
- Seslendirme: **Web Speech API** (tarayıcı tarafı, sunucu yükü yok).
- Resim kaynağı: Harici görsel API (anlık çekim, DB'de saklanmaz).

### 6.2 Öğrenilenler Havuzu Ekranı
- `ViewCount == 2` olan tüm kelime ve cümleleri listeler.
- Seviye filtreleme (A1–C2).
- Kelime / cümle sekme ayrımı.
- Her kaydın yanında **Geri Al** butonu.

### 6.3 Karışık Tekrar Modu
- Sadece Öğrenilenler Havuzu'ndaki kayıtlardan rastgele çekim.
- Limit yoktur, sonsuz antrenman.
- Bu moddaki gösterimler `ViewCount` ve `LastReviewed` değerlerini **değiştirmez**.
- SQL: `ORDER BY RANDOM() LIMIT N` (PostgreSQL).

---

## 7. Veri Modelleri (Final)

Tüm string alanlarda `[Required]` ve uygun `[MaxLength]` konulur. EF Core, PostgreSQL'de varsayılan olarak `text` (sınırsız) oluşturduğu için max length zorunludur.

### 7.1 AppUser

| Alan | Tip | Kural |
|---|---|---|
| `Id` | int | PK |
| `Username` | string | Required, MaxLength(50), **Unique index** |
| `PasswordHash` | string | Required, MaxLength(200). Düz metin parola **asla tutulmaz**. |
| `Role` | string | Required, MaxLength(10). Değerler: `"Admin"`, `"User"`. |
| `CurrentLevel` | string | Required, MaxLength(2). Değerler: `A1`, `A2`, `B1`, `B2`, `C1`, `C2`. Kullanıcının şu an çalıştığı seviye. |
| `HighestReachedLevel` | string | Required, MaxLength(2). Kullanıcının otomatik geçişle ulaştığı en üst seviye. Manuel geri dönüşte değişmez. |
| `DailyWordLimit` | int | `[Range(1, 50)]`, default 10 |
| `DailySentenceLimit` | int | `[Range(1, 25)]`, default 5 |

### 7.2 Word

| Alan | Tip | Kural |
|---|---|---|
| `Id` | int | PK |
| `EnglishText` | string | Required, MaxLength(100) |
| `TurkishText` | string | Required, MaxLength(200) |
| `Level` | string | Required, MaxLength(2). A1–C2. |

### 7.3 Sentence

| Alan | Tip | Kural |
|---|---|---|
| `Id` | int | PK |
| `EnglishText` | string | Required, MaxLength(300) |
| `TurkishText` | string | Required, MaxLength(500) |
| `Level` | string | Required, MaxLength(2). A1–C2. |

### 7.4 UserWordProgress

| Alan | Tip | Kural |
|---|---|---|
| `Id` | int | PK |
| `AppUserId` | int | FK → `AppUser.Id` |
| `WordId` | int | FK → `Word.Id` |
| `ViewCount` | int | default 0. Geçerli değerler: 0, 1, 2. |
| `LastReviewed` | DateTime? | UTC. İçerik kullanıcıya en son gösterildiği tarih. |
| `AppUser` | AppUser | Navigation property |
| `Word` | Word | Navigation property |

**Composite unique index:** `(AppUserId, WordId)`

### 7.5 UserSentenceProgress

| Alan | Tip | Kural |
|---|---|---|
| `Id` | int | PK |
| `AppUserId` | int | FK → `AppUser.Id` |
| `SentenceId` | int | FK → `Sentence.Id` |
| `ViewCount` | int | default 0. Geçerli değerler: 0, 1, 2. |
| `LastReviewed` | DateTime? | UTC. İçerik kullanıcıya en son gösterildiği tarih. |
| `AppUser` | AppUser | Navigation property |
| `Sentence` | Sentence | Navigation property |

**Composite unique index:** `(AppUserId, SentenceId)`

---

## 8. DbContext

**Sınıf:** `EnglishCore.Data.ApplicationDbContext`

**DbSet'ler:**
- `AppUsers`
- `Words`
- `Sentences`
- `UserWordProgresses`
- `UserSentenceProgresses`

**OnModelCreating Fluent API kuralları:**
- `AppUser.Username` → unique index.
- `UserWordProgress (AppUserId, WordId)` → composite unique index.
- `UserSentenceProgress (AppUserId, SentenceId)` → composite unique index.
- Cascade davranışları default (FK silindiğinde ilişkili Progress kaydı silinir).

---

## 9. Güvenlik

- **Şifreler (geliştirme aşaması — geçici):** `AppUser.PasswordHash` alanına şimdilik **düz metin** olarak yazılır. Sade implementasyon için auth akışı önce düz metinle kurulur. Auth tamamlandıktan sonra veya production'a alınmadan önce **BCrypt.Net-Next** ile hash'lemeye geçilecek (yol haritası adımı). Production'da düz metin parola asla DB'ye yazılmaz.
- **Kimlik doğrulama:** ASP.NET Core Cookie Authentication.
- **Yetkilendirme:** `[Authorize]` ve `[Authorize(Roles = "Admin")]` attribute'ları.
- **CSRF:** Tüm POST formlarında `@Html.AntiForgeryToken()` + controller'da `[ValidateAntiForgeryToken]`.
- **Input validation:** Model-level `[Required]`, `[Range]`, `[MaxLength]` zorlanır. Controller `ModelState.IsValid` kontrolü yapar.
- **SQL Injection:** EF Core parametreli sorgu kullandığı için risk yoktur. Ham SQL kullanılmaz.

---

## 10. Yapılandırma

- **Connection string:** `appsettings.json` içinde `ConnectionStrings:DefaultConnection` olarak tutulur. Geliştirme bilgisi `appsettings.Development.json`'da override edilir.
- **DI kaydı:** `Program.cs` içinde:
  - `builder.Services.AddDbContext<ApplicationDbContext>(opts => opts.UseNpgsql(...))`
  - Cookie auth servisleri
  - Authorization servisleri
- **Migration komutları (PMC):**
  - `Add-Migration <isim>`
  - `Update-Database`

---

## 11. Geliştirme Sırası (Yol Haritası)

1. ✅ `ApplicationDbContext` oluştur.
2. ✅ Modellerde validation/constraint/navigation eksiklerini tamamla, `PasswordHash` alanına geç.
3. ✅ `appsettings.json` connection string ekle.
4. ✅ `Program.cs` üzerinde DbContext kaydı + cookie auth + authorization.
5. ✅ İlk migration: `InitialCreate`.
6. ✅ Auth controller'ları: Register / Login / Logout (+ AccessDenied + seed admin).
7. ✅ Admin paneli: Word ve Sentence CRUD (rol bazlı `[Authorize(Roles="Admin")]`).
8. ⏳ User dashboard: günlük limit ayarı + seviye seçimi.
9. ⏳ Günlük Kart akışı (yeni + tekrar listesi mantığı).
10. ⏳ Öğrenilenler Havuzu ekranı + Geri Al butonu.
11. ⏳ İlerleme Çizelgesi widget'ı.
12. ⏳ Karışık Tekrar Modu.
13. ⏳ UI iyileştirme (Tinder-card animasyon, görsel API entegrasyonu, Web Speech API).
14. ⏳ **Şifre hash'lemeyi etkinleştir** (BCrypt.Net-Next ile) — auth tamamlandıktan sonra veya production öncesi.
