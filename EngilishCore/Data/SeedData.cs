using EnglishCore.Models;
using Microsoft.EntityFrameworkCore;

namespace EnglishCore.Data
{
    // Uygulama açılırken çağrılır.
    // - Admin yoksa default bir admin oluşturur
    // - Words tablosu boşsa A1 (günlük kelimeler) + B1 (yazılım terimleri) seed eder
    public static class SeedData
    {
        // Program.cs'ten çağrılır, app.Run()'dan önce
        public static async Task SeedAsync(ApplicationDbContext db)
        {
            // Bekleyen migration varsa otomatik uygula (dev kolaylığı)
            await db.Database.MigrateAsync();

            // İki seed işlemini sırayla çalıştır
            await SeedAdminAsync(db);
            await SeedWordsAsync(db);
        }

        // ===================== Admin =====================
        // Sistemde admin yoksa default bir admin hesabı oluşturur (username: admin / password: admin)
        private static async Task SeedAdminAsync(ApplicationDbContext db)
        {
            var hasAdmin = await db.AppUsers.AnyAsync(u => u.Role == "Admin");
            if (hasAdmin) return;

            db.AppUsers.Add(new AppUser
            {
                Username = "admin",
                PasswordHash = "admin",              // ŞİMDİLİK DÜZ METİN, sonra BCrypt
                Role = "Admin",
                CurrentLevel = "A1",                  // Admin için seviye önemsiz
                HighestReachedLevel = "A1",
                DailyWordLimit = 10,
                DailySentenceLimit = 5
            });
            await db.SaveChangesAsync();
        }

        // ===================== Words =====================
        // Words tablosu boşsa default kelime havuzunu seed eder.
        // İçi dolu ise tekrar seed etmez (idempotent davranış).
        private static async Task SeedWordsAsync(ApplicationDbContext db)
        {
            if (await db.Words.AnyAsync()) return;

            var words = new List<Word>();

            // A1 — genel günlük İngilizce kelimeler (en yaygın 600+ kelime)
            foreach (var (eng, tr) in ParseDelimited(A1WordsRaw))
                words.Add(new Word { EnglishText = eng, TurkishText = tr, Level = "A1" });

            // B1 — yazılım/teknoloji terimleri (100 adet, daha teknik orta seviye)
            foreach (var (eng, tr) in ParseDelimited(B1TechWordsRaw))
                words.Add(new Word { EnglishText = eng, TurkishText = tr, Level = "B1" });

            db.Words.AddRange(words);
            await db.SaveChangesAsync();
        }

        // Pipe karakteri ile ayrılmış string'i (english|turkish) pair listesine dönüştürür.
        // Boş satırları ve hatalı satırları atlar.
        private static IEnumerable<(string eng, string tr)> ParseDelimited(string raw)
        {
            foreach (var line in raw.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                var trimmed = line.Trim();
                if (string.IsNullOrWhiteSpace(trimmed)) continue;

                var idx = trimmed.IndexOf('|');
                if (idx <= 0) continue;

                var eng = trimmed.Substring(0, idx).Trim();
                var tr = trimmed.Substring(idx + 1).Trim();
                if (string.IsNullOrEmpty(eng) || string.IsNullOrEmpty(tr)) continue;

                yield return (eng, tr);
            }
        }

        // ===================== A1 KELIME LISTESI (GÜNLÜK) =====================
        // Her satır: english|turkish
        private const string A1WordsRaw = @"
a|bir
ability|kabiliyet, yetenek, beceri
able|yapabilmek, yapabilen
about|hakkında, ilgili, konusunda
above|yukarıda
accept|kabul etmek
according|göre
account|hesap, açıklama
across|karşısında
act|eylem, davranış
action|eylem, etki, hareket
activity|faaliyet, etkinlik
actually|aslında, gerçekte
add|eklemek, ilave etmek
address|adres
administration|yönetim, idare
admit|kabul etmek, itiraf etmek
adult|yetişkin, erişkin
affect|etkilemek, etki etmek
after|sonra, ardından, daha sonra
afternoon|öğleden sonra
afterwards|ondan sonra
again|tekrar, yeniden
against|karşı
age|yaş
agency|ajans, acenta
agent|ajan
ago|önce, evvel
agree|katılmak, hem fikir olmak
agreement|anlaşma, sözleşme
ahead|önde, ileri
air|hava
all|her şey, tüm, bütün
all right|peki
allow|izin vermek
almost|neredeyse, hemen hemen
alone|yalnız, tek başına
along|boyunca, yanı sıra
already|zaten
also|ayrıca, hem de
although|rağmen, karşın, gerçi
always|her zaman, daima
american|amerikan
among|arasında, içinde
amount|miktar, tutar
analysis|analiz, çözümleme
and|ve, ile
animal|hayvan
another|diğeri
answer|cevap, yanıt
any|herhangi, hiç, her
anyone|kimse, herhangi biri, hiç kimse
anything|bir şey, herhangi bir şey, hiçbir şey
anyway|her halükârda
anywhere|herhangi bir yere
appear|görünmek, gözükmek
apple|elma
apply|uygulamak, başvurmak
approach|yaklaşım, yaklaşma
april|nisan
area|alan, bölge
argue|tartışmak
arm|kol
around|etrafında
arrive|varmak, ulaşmak
art|sanat
article|makale, yazı
artist|sanatçı
as|gibi, olarak
ask|sormak, istemek
assume|varsaymak, farzetmek
attack|saldırı, atak
attention|dikkat
attorney|avukat, vekil
audience|seyirci, izleyici
author|yazar
authority|yetki, otorite
available|müsait
avoid|önlemek, kaçınmak
away|uzak, uzakta
baby|bebek
back|geri
bad|kötü
bag|sırt çantası
ball|top
bank|banka
bar|bar
base|üs, temel, baz
be|olmak, var olmak
beat|dövmek, yenmek
beautiful|güzel
because|çünkü
become|olmak, haline gelmek
bed|yatak
before|önce
begin|başlamak, başlatmak
behavior|davranış, tutum, hareket
behind|arkasında, gerisinde
believe|inanmak
benefit|yarar, fayda
best|en iyi, en iyisi, en
better|daha iyi, daha güzel, daha iyi şekilde
between|arasında, aralarında
beyond|ötesinde, öte
big|büyük
bill|fatura
billion|milyar
black|siyah
blood|kan
blue|mavi
board|yazı tahtası
body|vücut
book|kitap
born|doğmuş, doğum
both|her ikisi de, ikisi de
box|kutu
boy|erkek çocuk
break|kırmak
bring|getirmek
brother|erkek kardeş
budget|bütçe
build|inşa etmek
building|bina
business|iş
but|fakat, ama, lakin
buy|satın almak
by|tarafından, ile, göre, vasıtasıyla
call|aramak
camera|kamera
campaign|kampanya
can|yapabilmek
cancer|kanser
candidate|aday
capital|başkent
car|araba
card|kart
care|bakım
career|kariyer
carry|taşımak
case|dava
catch|yakalamak
cause|sebep olmak
cell|hücre
center|merkez
central|merkezi
century|yüzyıl
certain|belirli, kesin
certainly|kesinlikle
chair|sandalye
challenge|meydan okuma
chance|şans
change|değişiklik
character|karakter
charge|şarj etmek
check|kontrol etmek, denetlemek
child|çocuk
choice|seçim, seçenek
choose|seçmek
church|kilise
citizen|vatandaş
city|şehir
civil|sivil
claim|iddia etmek, iddia
class|sınıf
clear|temiz, açık
clearly|açıkça
close|kapatmak
coach|koç
cold|soğuk
collection|toplamak
college|kolej
color|renk
come|gelmek
commercial|ticari
common|ortak
community|topluluk
company|şirket
compare|karşılaştırmak
computer|bilgisayar
concern|ilişkisi olmak, ait olmak, ilgilendirmek
condition|şart
conference|konferans
congress|kongre
consider|dikkate almak
consumer|tüketici
contain|içermek
continue|devam etmek
control|kontrol
cost|maliyet
country|ülke
couple|çift
course|ders
court|mahkeme
cover|örtmek
create|yaratmak
crime|suç
cultural|kültürel
culture|kültür
cup|fincan
current|şu andaki
customer|müşteri
cut|kesmek
dark|karanlık
data|veri
daughter|kız evlat
day|gün
dead|ölü
deal|anlaşmak
death|ölüm
debate|müzakere, tartışma
decade|on yıl
decide|karar vermek
decision|karar
deep|derin
defense|savunma
degree|derece
democrat|demokrat
democratic|demokratik
describe|tanımlamak
design|tasarım
despite|rağmen, aksine
detail|ayrıntı
determine|belirlemek
develop|geliştirmek
development|gelişim, kalkınma
die|ölmek
difference|fark
different|farklı
difficult|zor
dinner|akşam yemeği
direction|yön
director|yönetmen
discover|keşfetmek
discuss|tartışmak
discussion|tartışma
disease|hastalık
do|yapmak
doctor|doktor
dog|köpek
door|kapı
down|aşağı
draw|çizmek
dream|rüya
drive|sürmek
drop|düşürmek
drug|uyuşturucu
during|sırasında
each|her biri
early|erken
east|doğu
easy|kolay
eat|yemek yemek
economic|ekonomik
economy|ekonomi
edge|kenar, kıyı
education|eğitim
effect|efekt
effort|çaba
eight|sekiz
either|ya
election|seçim
else|başka
employee|çalışan
end|son
energy|enerji
enjoy|keyif almak
enough|yeterli
enter|girmek, katılmak
entire|tamamı
environment|çevre
environmental|çevreci
especially|özellikle
establish|kurmak
evening|akşam
event|olay
ever|hiç
every|her
everybody|herkes
everyone|herkes, her biri
everything|her şey
evidence|delil
exactly|kesinlikle
example|örnek
executive|yönetici
exist|var olmak
expect|ummak
experience|deneyim
expert|uzman
explain|açıklamak
eye|göz
face|yüz
fact|gerçek
factor|faktör
fail|başarısız
fall|düşmek
family|aile
far|uzak
fast|hızlı
father|baba
fear|korku
federal|federal
feel|hissetmek
feeling|duygu
few|az
field|tarla, alan
fight|kavga
figure|şekil
fill|doldurmak
film|film
final|final, nihai
finally|en sonunda
financial|mali
find|bulmak
fine|iyi, güzel
finger|parmak
finish|bitiş
fire|ateş
firm|firma
first|ilk
fish|balık
five|beş
floor|zemin
fly|uçmak
focus|odaklanmak
follow|takip etmek
food|gıda, yiyecek
foot|ayak
for|için
force|kuvvet
foreign|yabancı
forget|unutmak
form|form
former|önceki, eski
forward|ileri
four|dört
free|ücretsiz
friend|arkadaş
front|ön
full|tam
fund|fon, sermaye
future|gelecek
game|oyun
garden|bahçe
gas|gaz
general|genel
generation|nesil
get|almak
girl|kız
give|vermek
glass|bardak
go|gitmek
goal|hedef
good|iyi
government|hükümet
great|harika
green|yeşil
ground|zemin
group|grup
grow|büyümek
growth|büyüme
guess|tahmin
gun|silah
guy|adam
hair|saç
half|yarım
hand|el
hang|asmak
happen|olmak
happy|mutlu
hard|zor
have|sahip olmak
he|o (erkek)
head|kafa
health|sağlık
hear|duymak
heart|kalp
heat|sıcaklık
heavy|ağır
help|yardım
her|ona (kadın)
here|burada
herself|kendini
high|yüksek
him|onu (erkek)
himself|kendisi
his|onun (erkek)
history|tarih
hit|çarpmak
hold|tutmak
home|ev
hope|umut
hospital|hastane
hot|sıcak
hotel|otel
hour|saat
house|ev
how|nasıl
however|ancak
huge|kocaman
human|insan
hundred|yüz
husband|koca
idea|fikir
identify|belirlemek, tanımak, tanımlamak
if|eğer
image|görüntü, resim
imagine|hayal etmek
impact|etki
important|önemli
improve|iyileştirmek, iyi hale getirmek
in|içinde
include|dahil etmek
including|dahil olmak üzere
increase|artırmak
indeed|aslında
indicate|belirtmek
individual|bireysel
industry|endüstri
information|bilgi
inside|içeride
instead|yerine
institution|kurum
interest|faiz
interesting|ilginç
international|uluslararası
interview|röportaj
into|içine
investment|yatırım
involve|içermek, kapsamak
issue|sorun
it|o (cansız)
item|öğe
its|onun (cansız)
itself|kendisi
job|meslek
join|katılmak
just|sadece
keep|tutmak
key|anahtar
kid|çocuk
kill|öldürmek
kind|tür
kitchen|mutfak
know|bilmek
knowledge|bilgi
land|arazi, toprak
language|dil
large|geniş
last|son
late|geç
later|sonra
laugh|gülmek
law|hukuk
lawyer|avukat
lay|yatırmak
lead|öncülük etmek
leader|lider
learn|öğrenmek
least|en az
leave|ayrılmak
left|ayrıldı
leg|bacak
legal|yasal
less|az
let|izin vermek
letter|mektup
level|seviye
lie|yalan
life|hayat
light|ışık
like|sevmek, beğenmek
likely|muhtemel
line|çizgi, sıra
list|liste
listen|dinlemek
little|küçük
live|canlı
local|yerel
long|uzun
look|bakmak
lose|kaybetmek
loss|kayıp
lot|çok
love|aşk
low|düşük
machine|makine
magazine|dergi
main|ana
maintain|sürdürmek
majority|çoğunluk
make|yapmak
man|adam, insan
manage|yönetmek
management|yönetim
manager|müdür, yönetici
many|çok
market|pazar
marriage|evlilik
material|malzeme
matter|konu, mesele
may|mayıs ayı
maybe|belki, olabilir
me|bana, beni
mean|anlamına gelmek
measure|ölçmek
media|medya
medical|tıbbi
meet|buluşmak
meeting|toplantı
member|üye
memory|hafıza
mention|değinmek, söz etmek
message|mesaj
method|yöntem
middle|orta
might|belki
military|askeri
million|milyon
mind|zihin
minute|dakika
miss|özlemek
mission|misyon
model|model
modern|modern
moment|an
money|para
month|ay
more|daha
morning|sabah
most|çoğu
mother|anne
mouth|ağız
move|hareket etmek
movement|hareket
movie|film
Mr|bay
Mrs|bayan
much|çok
music|müzik
must|zorunlu
my|benim
myself|kendim
name|isim
nation|ulus
national|ulusal
natural|doğal
nature|doğa
near|yakın
nearly|neredeyse
necessary|gerekli
need|ihtiyaç, gereksinim
network|ağ
never|asla
new|yeni
news|haber
newspaper|gazete
next|sonraki
nice|iyi, güzel
night|gece
no|hayır
none|yok, hayır, hiçbiri, hiçbir şey
nor|ne de
north|kuzey
not|değil
note|not
nothing|hiçbir şey
notice|fark etmek
now|şimdi
number|numara
occur|meydana gelmek
off|kapalı
offer|teklif etmek
office|ofis
officer|memur
official|resmi
often|sıklıkla
oil|yağ
okay|tamam
old|yaşlı, eski
on|üzerinde
once|bir zamanlar
one|bir
only|bir tek
onto|üstüne
open|açık
operation|ameliyat
opportunity|fırsat
option|seçenek
or|veya
order|sipariş, sipariş vermek
organization|organizasyon
other|diğer
others|diğerleri
our|bizim
out|dışarı
outside|dışında
over|bitti
own|kendi
owner|sahip
page|sayfa
pain|ağrı
painting|boyama
paper|kağıt
parent|ebeveyn
part|bölüm
participant|katılımcı
particular|özel, belirli
particularly|özellikle
partner|partner, ortak
party|parti
pass|geçmek
past|geçmiş
patient|hasta, sabırlı, hoşgörülü
pattern|desen
pay|ödemek
peace|barış
people|insanlar
per|başına, göre
perform|yapmak, uygulamak, yerine getirmek
performance|performans
perhaps|belki
period|periyot, dönem
person|kişi
personal|kişisel
phone|telefon
physical|fiziksel
pick|almak
picture|resim
piece|parça
place|yer, mekan
plan|plan
plant|bitki
play|oynamak
player|oyuncu
point|puan
police|polis
policy|politika
political|siyasi
politics|siyaset
poor|fakir
popular|popüler
population|nüfus
position|pozisyon
positive|pozitif
possible|mümkün
power|güç
practice|uygulama, pratik
prepare|hazırlamak
present|mevcut
president|devlet başkanı
pressure|basınç
pretty|güzel
prevent|engel olmak
price|fiyat
private|özel
probably|muhtemelen
problem|problem, sorun
process|süreç, işlem
produce|üretmek
product|ürün
production|üretim
professional|profesyonel
professor|profesör
program|program
project|proje
property|gayrimenkul
protect|korumak
prove|kanıtlamak
provide|sağlamak
public|halka açık
pull|çekmek
purpose|amaç
push|itmek
put|koymak
quality|kalite
question|soru
quickly|hızlı bir şekilde
quite|oldukça
race|yarış
radio|radyo
raise|yükseltmek
range|menzil, aralık
rate|oran
rather|daha doğrusu
reach|ulaşmak
read|okumak
ready|hazır
real|reel, gerçek
reality|gerçeklik
realize|fark etmek
really|gerçekten
reason|neden
receive|teslim almak
recent|son
recently|son günlerde
recognize|tanımak
record|kayıt
red|kırmızı
reduce|azaltmak, düşürmek
reflect|yansıtmak
region|bölge
relate|ilişki kurmak
relationship|ilişki
religious|dini
remain|kalmak, sürdürmek
remember|hatırlamak
remove|kaldırmak, uzaklaştırmak, ortadan kaldırmak
report|rapor
represent|temsil etmek
republican|cumhuriyetçi
require|gerekmek, gerektirmek
research|araştırma
resource|kaynak
respond|yanıtlamak
response|cevap vermek
responsibility|sorumluluk
rest|dinlenme
result|sonuç
return|dönüş
reveal|ortaya çıkartmak, açığa vurmak
rich|zengin
right|sağ
rise|yükselmek
risk|risk
road|karayolu, yol
rock|kaya
role|rol
room|oda
rule|kural
run|koşmak
safe|güvenli
same|aynı
save|kayıt etmek
say|söylemek
scene|sahne
school|okul
science|bilim
scientist|bilim insanı
score|skor
sea|deniz
season|sezon
seat|koltuk
second|ikinci
section|bölüm, kesit
security|güvenlik
see|görmek
seek|aramak
seem|görünmek
sell|satmak
send|göndermek
senior|kıdemli, yaşça büyük
sense|algı, duyu
series|dizi, seri
serious|ciddi
serve|servis
service|hizmet
set|ayarlamak
seven|yedi
several|birkaç
shake|sallamak
share|paylaşmak
she|o (kadın)
shoot|şut çekmek
short|kısa
shot|atış
should|meli, malı
shoulder|omuz
show|göstermek
side|yan
sign|işaret
significant|önemli
similar|benzer
simple|basit
simply|basitçe
since|dan beri
sing|şarkı söylemek
single|tek
sister|kız kardeş
sit|oturmak
site|alan
situation|durum
six|altı
size|boyut
skill|beceri
skin|cilt
small|küçük
smile|gülümseme
so|yani
social|sosyal
society|toplum
soldier|asker
some|bazı
somebody|birileri
someone|birisi
something|bir şey
sometimes|ara sıra
son|oğul
song|şarkı
soon|yakında
sort|çeşit
sound|ses
source|kaynak
south|güney
southern|güney, güneyli
space|boşluk
speak|konuşmak
special|özel
specific|özel
speech|konuşma
spend|harcamak
sport|spor
spring|bahar
staff|personel
stage|sahne
stand|ayakta durmak, direnmek
standard|standart
star|yıldız
start|başlama
state|devlet, durum
statement|beyan, ifade
station|istasyon
stay|kalmak
step|adım
still|hala
stock|stok
stop|dur, durmak
store|mağaza
story|öykü
strategy|strateji
street|sokak
strong|güçlü
structure|yapı
student|öğrenci
study|ders çalışma
stuff|şey, madde
style|stil
subject|konu
success|başarı
successful|başarılı
such|böyle
suddenly|aniden
suffer|acı çekmek
suggest|önermek
summer|yaz
support|destek
sure|emin
surface|yüzey
system|sistem
table|masa, tablo
take|almak
talk|konuşmak
task|görev
tax|vergi
teach|öğretmek
teacher|öğretmen
team|takım
technology|teknoloji
television|televizyon
tell|söylemek
ten|on
tend|yönelmek, eğilmek
term|terim
test|test
thank|teşekkür etmek
that|o
their|onların
them|onları
themselves|kendilerini
then|sonra
theory|teori
there|orada
these|bunlar
they|onlar
thing|şey
think|düşünmek
third|üçüncü
this|bu
those|bunlar
though|olsa da
thought|düşündü
thousand|bin
threat|tehdit
three|üç
through|vasıtasıyla
throughout|boyunca
throw|atmak
thus|böylece
time|zaman
to|mastar (mek, mak), karşı
today|bugün
together|birlikte
tonight|bu gece
too|çok, fazla
top|üst
total|toplam, tüm
tough|sert
toward|-e doğru
town|kasaba
trade|ticaret
traditional|geleneksel
training|eğitim, antrenman
travel|seyahat
treat|tedavi etmek
treatment|tedavi
tree|ağaç
trial|deneme, test
trip|yolculuk
trouble|sorun
true|doğru
truth|hakikat
try|denemek
turn|çevirmek
two|iki
type|tip, tür, model
under|altında
understand|anlamak
unit|ünite
until|-a kadar
up|yukarı
upon|üzerinde
us|bize
use|kullanmak
usually|genellikle
value|değer
various|çeşitli
very|çok
victim|kurban
view|görünüm
violence|şiddet
visit|ziyaret etmek
voice|ses
vote|oy
wait|beklemek
walk|yürümek
wall|duvar
want|istemek
war|savaş
watch|izlemek
water|su
way|yol, yön, yöntem
we|biz
weapon|silah
wear|giyinmek
week|hafta
weight|ağırlık
well|daha iyi
west|batı
western|batıda
what|ne
whatever|her neyse
when|ne zaman
where|nerede
which|hangi
while|iken
white|beyaz
who|kim
whom|kime
whose|kimin
why|niçin
wide|geniş
wife|kadın eş
win|kazanmak
wind|rüzgar
window|pencere
wish|dilek, dilemek
with|ile
within|içinde
without|olmadan
woman|kadın
wonder|merak etmek, şaşmak
word|kelime
work|çalışmak
worker|işçi
world|dünya
worry|endişe etmek
would|cekti, caktı
write|yazmak
writer|yazar
wrong|yanlış
yard|avlu
yarn|iplik
year|yıl
yearly|yıllık
yelling|seslenmek
yes|evet
yesterday|dün
yet|henüz
you|sen
young|genç
your|senin
yourself|kendin
youth|gençlik
zebra|zebra
zeppelin|zeplin
zipper|fermuar
zodiac|burçlar kuşağı
zombie|zombi
zoo|hayvanat bahçesi
zoology|hayvan bilimi
zoom|yakınlaşmak, zoom yapmak
";

        // ===================== B1 YAZILIM TERIMLERI (100 adet) =====================
        // Kullanıcı listesindeki kelimelerle çakışanlar (process, system, network, method, memory, source, class, control, design, range, event, response, test) buradan çıkarıldı.
        // Her satır: english|turkish
        private const string B1TechWordsRaw = @"
algorithm|algoritma
api|uygulama programlama arayüzü
array|dizi
authentication|kimlik doğrulama
backend|arka uç
bandwidth|bant genişliği
binary|ikili sistem
boolean|mantıksal değer
branch|dal, kol
browser|web tarayıcısı
buffer|tampon bellek
bug|yazılım hatası
cache|önbellek
callback|geri çağırma
client|istemci
cloud|bulut
code|kod
commit|kaydetmek
compile|derlemek
component|bileşen
configuration|yapılandırma
console|konsol
container|kapsayıcı
controller|denetleyici, kontrolcü
cookie|çerez
database|veritabanı
debug|hata ayıklama
deploy|dağıtmak, yayına almak
developer|yazılım geliştirici
device|cihaz
directory|dizin
domain|alan adı
download|indirmek
driver|sürücü yazılımı
encryption|şifreleme
endpoint|uç nokta
engine|motor
error|hata
exception|istisna
execute|çalıştırmak
file|dosya
filter|filtre
firewall|güvenlik duvarı
folder|klasör
framework|çerçeve, yazılım iskeleti
frontend|ön uç
function|işlev, fonksiyon
gateway|ağ geçidi
hardware|donanım
hash|özet, karma
header|başlık
host|sunucu makine
input|giriş, girdi
install|kurmak, yüklemek
instance|örnek
integer|tam sayı
interface|arayüz
internet|internet
keyboard|klavye
library|kütüphane
license|lisans
link|bağlantı
log|günlük kayıt
loop|döngü
migration|göç, taşıma
module|modül
mouse|fare
node|düğüm
object|nesne
operator|işleç
output|çıkış, çıktı
package|paket
parameter|parametre
password|şifre
patch|yama
permission|izin
pipeline|iş hattı
plugin|eklenti
port|port, kapı
protocol|protokol
proxy|vekil sunucu
query|sorgu
queue|kuyruk
ram|rastgele erişimli bellek
release|sürüm
repository|depo
request|istek
runtime|çalışma zamanı
script|betik
server|sunucu
session|oturum
software|yazılım
stack|yığın
storage|depolama
string|dizgi, metin
syntax|sözdizimi
template|şablon
thread|iş parçacığı
token|belirteç
upload|yüklemek
url|bağlantı adresi
user|kullanıcı
variable|değişken
version|sürüm
";
    }
}
