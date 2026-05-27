// Kelime kart akışı + seslendirme
// - Bir seferde tek kart görünür, navigasyon butonlarıyla geçiş
// - Karta tıklayınca CSS 3D flip ile arka yüze döner (Türkçe karşılık)
// - Sesli Oku butonu: önce Free Dictionary API'den native ses dener,
//   bulamazsa browser'ın Web Speech API (speechSynthesis) ile TTS yapar
(function () {
    'use strict';

    const cards = document.querySelectorAll('.study-card');
    const indexLabel = document.getElementById('cardIndex');
    const totalLabel = document.getElementById('cardTotal');
    const btnNext = document.getElementById('btnNext');
    const btnPrev = document.getElementById('btnPrev');

    // Kart yoksa script bir şey yapmaz (Index sayfası boş havuz durumu)
    if (cards.length === 0) return;

    let currentIndex = 0;

    // Sadece istenen index'teki kartı görünür yap, diğerlerini gizle
    // Yeni karta geçildiğinde flip durumunu sıfırla — kart her zaman İngilizce yüzü ile başlasın
    function showCard(index) {
        cards.forEach((c, i) => {
            c.style.display = i === index ? 'block' : 'none';
            c.classList.remove('flipped');
        });
        indexLabel.textContent = String(index + 1);
        btnPrev.disabled = index === 0;
        btnNext.disabled = index === cards.length - 1;
    }

    // Sonraki butonu — son karttaysa durur
    btnNext.addEventListener('click', () => {
        if (currentIndex < cards.length - 1) {
            currentIndex++;
            showCard(currentIndex);
        }
    });

    // Önceki butonu — ilk karttaysa durur
    btnPrev.addEventListener('click', () => {
        if (currentIndex > 0) {
            currentIndex--;
            showCard(currentIndex);
        }
    });

    // Kart tıklamaları — flip
    // Buton tıklamasında flip yapmıyoruz (kullanıcı sesli oku basınca kart çevrilmesin diye)
    cards.forEach(card => {
        card.addEventListener('click', (e) => {
            if (e.target.closest('button')) return;
            card.classList.toggle('flipped');
        });
    });

    // ===== SESLENDİRME =====
    // Her "Sesli Oku" butonuna click listener ekle
    document.querySelectorAll('.btn-speak').forEach(btn => {
        btn.addEventListener('click', async (e) => {
            // Flip event'ini engelle — sadece ses oynatsın, kart çevrilmesin
            e.stopPropagation();
            const word = btn.dataset.word;
            if (!word) return;

            // Buton geçici olarak disable + yükleme görseli
            const originalHtml = btn.innerHTML;
            btn.disabled = true;
            btn.innerHTML = '<i class="bi bi-hourglass-split me-1"></i>Yükleniyor...';

            try {
                // 1. Dictionary API'den native ses URL'i al
                const audioUrl = await fetchDictionaryAudio(word);
                if (audioUrl) {
                    // Native ses bulundu — mp3 olarak çal
                    const audio = new Audio(audioUrl);
                    audio.play().catch(() => {
                        // Audio play başarısız (autoplay policy vb.) — fallback'e geç
                        speakWithBrowser(word);
                    });
                } else {
                    // Dictionary API'de ses yok — Web Speech API ile sentezle
                    speakWithBrowser(word);
                }
            } catch (err) {
                // Beklenmedik hata — fallback'e geç
                console.warn('Dictionary API hata:', err);
                speakWithBrowser(word);
            } finally {
                // Butonu eski haline getir
                btn.disabled = false;
                btn.innerHTML = originalHtml;
            }
        });
    });

    // Free Dictionary API'den (dictionaryapi.dev) kelimenin audio URL'ini çek
    // Endpoint: GET https://api.dictionaryapi.dev/api/v2/entries/en/{word}
    // Dönüş: array of entries → phonetics → audio (varsa)
    async function fetchDictionaryAudio(word) {
        try {
            const url = `https://api.dictionaryapi.dev/api/v2/entries/en/${encodeURIComponent(word)}`;
            const res = await fetch(url);
            if (!res.ok) return null;
            const data = await res.json();
            if (!Array.isArray(data)) return null;

            // Tüm entry'lere ve onların phonetics dizilerine bak — ilk dolu audio URL'i kullan
            for (const entry of data) {
                const phonetics = entry.phonetics || [];
                for (const ph of phonetics) {
                    if (ph.audio && typeof ph.audio === 'string' && ph.audio.startsWith('http')) {
                        return ph.audio;
                    }
                }
            }
            return null;
        } catch {
            return null;
        }
    }

    // Browser'ın yerleşik TTS motorunu kullan (sunucu yükü yok, ek paket yok)
    // Bütün modern tarayıcılar destekler (Chrome, Firefox, Safari, Edge)
    function speakWithBrowser(word) {
        if (!('speechSynthesis' in window)) {
            console.warn('Bu tarayıcı Web Speech API desteklemiyor.');
            return;
        }
        // Önceki konuşma varsa kes (kullanıcı hızlı tıklarsa biriksin)
        window.speechSynthesis.cancel();

        const utterance = new SpeechSynthesisUtterance(word);
        utterance.lang = 'en-US';
        utterance.rate = 0.9;   // Biraz yavaş — daha anlaşılır
        utterance.pitch = 1.0;
        window.speechSynthesis.speak(utterance);
    }

    // Sayfa açılınca ilk kartı göster
    showCard(0);
})();
