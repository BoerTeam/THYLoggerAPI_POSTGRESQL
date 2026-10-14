# Cool Dolly – Kullanıcı Kabul Testi (UAT)

Kaynak: ilk kurulum sonrası müşteri tespitleri (`cool dolly tespit.xlsx`) ve GitHub issue'ları.
Test edilen sürüm: **v1.0.1**

**Ön koşullar**
- Test kullanıcısı: `DASHBOARD_VIEW`, `DOLLY_VIEW` ve `EXPORT_EXCEL` yetkili (veya Admin).
- En az 1 dolly tanımlı, cihaz veri gönderiyor; son 24 saatte sıcaklık, nem, GPS ve boş/dolu kaydı var.
- Test sırasında kullanılacak cihaz/dolly: `____________`  Test tarihi: `____________`  Tester: `____________`

**Sonuç değerleri:** ✅ Geçti · ❌ Kaldı · ⏸ Bloklu / uygulanamaz

---

## 1. Grafikler

| ID | Issue | Adımlar | Beklenen sonuç | Sonuç | Not |
|---|---|---|---|---|---|
| G-01 | #11 | Ana sayfada bir dolly kartına tıkla. | Detay penceresi açılır; grafikler **son 24 saat** verisini gösterir. "Son 24 Saat" butonu seçili. | | |
| G-02 | #11 | "Son 1 Saat" ve "Son 6 Saat" butonlarına tıkla. | Grafikler seçilen aralığa göre yenilenir; çubukta kayıt sayısı görünür. | | |
| G-03 | #11 | Başlangıç ve bitiş tarih-saat seç, **Uygula**'ya tıkla. | Grafikler seçilen aralığı gösterir. 24 saatten uzun aralıkta etiketlerde `gg/aa SS:dd` görünür. | | |
| G-04 | #11 | Bitişi başlangıçtan önce seçip Uygula'ya tıkla. | "Geçerli bir aralık seçin" uyarısı çıkar, grafik bozulmaz. | | |
| G-05 | #12 | Nem grafiğine bak (değer değişimi olan bir aralıkta). | Y ekseni sabit **0–100**; çizgi gerçek değişimi gösterir. | | |
| G-06 | #13 | Sıcaklık grafiğine bak. | Y ekseni en az **0–25** görünür; çizgi değişir. Değer 25'i aşarsa eksen genişler. | | |

## 2. Harita ve konum

| ID | Issue | Adımlar | Beklenen sonuç | Sonuç | Not |
|---|---|---|---|---|---|
| H-01 | #14 | Dolly detayını aç, haritaya bak. Dolly'yi hareket ettir (veya cihazdan yeni konum gönder). | Konum işareti **sayfa yenilenmeden** ≤ 10 sn içinde yeni konuma geçer. İşaretin etiketinde dolly adı ve koordinat yazar. | | |
| H-02 | #14 | Dolly'yi harita görünümünün dışına çıkar. | Harita işareti takip eder. | | |
| H-03 | #15 | `Dashboard/wwwroot/map/locations.json` dosyasına bir lokasyonun lat/lng değerini yaz, sayfayı yenile, detayı aç. | Lokasyon renkli daire ve adıyla çizilir. *(Koordinatlar dolana kadar ⏸.)* | | |

## 3. Canlı güncelleme ve pencere davranışı

| ID | Issue | Adımlar | Beklenen sonuç | Sonuç | Not |
|---|---|---|---|---|---|
| C-01 | #16 | Dolly detayını aç ve **2 dakika** bekle. | Pencere kendiliğinden kapanmaz. | | |
| C-02 | #23 | Ana sayfada kartları izle; cihazdan yeni sıcaklık gönder. | Kartlardaki sıcaklık/nem **sayfa yenilenmeden** ≤ 10 sn içinde güncellenir; kartın altındaki "son güncelleme" zamanı ilerler. | | |
| C-03 | #23 | Kapıyı 2–3 dk açık bırak, kartı ve grafiği izle. | Sıcaklık değişimi kartta ve grafikte görünür. Değişmiyorsa **veritabanındaki ham kayıtları** kontrol et ve Not'a yaz (sensör/cihaz kaynaklı mı, sistem kaynaklı mı?). | | |
| C-04 | #14 | Detay açıkken cihazdan yeni veri gönder. | Boş/Dolu kutusu ve grafikler pencere kapanmadan güncellenir (grafik ≤ 30 sn). | | |

## 4. Tarih formatı

| ID | Issue | Adımlar | Beklenen sonuç | Sonuç | Not |
|---|---|---|---|---|---|
| T-01 | #17 | Tarayıcı dili İngilizce (US) iken detay penceresindeki Excel ve grafik tarih alanlarına bak. | Tarihler **gg/aa/yyyy SS:dd** görünür. | | |
| T-02 | #17 | İndirilen Excel'deki tarihlere bak. | `gg.aa.yyyy SS:dd:ss` formatında. | | |

## 5. Excel raporu

| ID | Issue | Adımlar | Beklenen sonuç | Sonuç | Not |
|---|---|---|---|---|---|
| E-01 | #18 | Excel formunda son 1 saatlik aralık seçip indir. | Yalnızca seçilen aralıktaki kayıtlar gelir (ilk/son satır saatlerini kontrol et). | | |
| E-02 | #19 | Aynı Excel'i aç. | **Sıcaklık, Nem ve GPS Konum** sayfalarının üçü de dolu. | | |
| E-03 | #18 | Başka bir dolly seçip indir. | Yalnızca o dolly'nin kayıtları gelir. | | |
| E-04 | #20 | Başlangıcı 2 yıl öncesine ayarla (veri varsa) ve indir. | Rapor makul sürede (≤ 1 dk) iner; veri varsa 2 yıl öncesine kadar gelir. Süreyi Not'a yaz. | | |
| E-05 | #21 | Excel'deki **Boş-Dolu Durumu** sayfasına bak. | Her satır bir durum aralığı: dolly, başlangıç, bitiş, Boş/Dolu, min/ort/maks sıcaklık, nem, başlangıç/bitiş koordinatı. | | |
| E-06 | #21 | Bir dolly'yi boş → dolu yap, 5 dk sonra rapor al. | Sayfada yeni bir "Dolu" aralığı oluşur; bu aralığın sıcaklık/nem/konum değerleri o döneme aittir. | | |
| E-07 | #21 | Seçilen aralıkta hiç boş/dolu kaydı olmayan bir aralık seç. | Boş-Dolu Durumu sayfası yalnızca başlıklarla gelir, hata vermez. | | |

## 6. Boş/Dolu sensörü (debounce)

Ayar: `appsettings.json > DoluBosDebounce:RequiredConsecutive` (varsayılan 3; 1 = kapalı).
Cihazın gönderim aralığı: `______` sn

| ID | Issue | Adımlar | Beklenen sonuç | Sonuç | Not |
|---|---|---|---|---|---|
| D-01 | #22 | Dolly'yi boşken kapıyı aç/kapat (3–4 kez, hızlı). | Kapı hareketi tek başına dolu/boş durumunu değiştirmez; anlık yanlış okumalar kaydedilmez. | | |
| D-02 | #22 | Dolly'yi gerçekten doldur ve bekle. | Durum, gönderim aralığının ~3 katı süre içinde **Dolu** olur. | | |
| D-03 | #22 | Dolly'yi boşalt ve bekle. | Durum **Boş** olur. | | |
| D-04 | #22 | D-02 sırasında durum **hiç değişmiyorsa**: cihaz sadece değişimde mi gönderiyor kontrol et. | Evetse `RequiredConsecutive` değerini 1 yapıp sunucuyu yeniden başlat ve testi tekrarla. Sonucu Not'a yaz. | | |

## 7. Sıcaklık doğruluğu

| ID | Issue | Adımlar | Beklenen sonuç | Sonuç | Not |
|---|---|---|---|---|---|
| S-01 | #24 | Dolly üzerindeki gösterge ile dashboard'daki sıcaklığı **aynı anda** karşılaştır; en az 3 farklı zamanda tekrarla. Gösterge, sistem değeri ve (varsa) ham mA değerini kaydet. | Fark müşterinin kabul ettiği toleransta (belirlenecek: `____ °C`). Fark sistematikse sensör aralığını (4 mA ve 20 mA kaç °C) bildir; kalibrasyon ayarı güncellenir. | | |

| Ölçüm | Saat | Dolly göstergesi (°C) | Sistem (°C) | Ham mA | Fark |
|---|---|---|---|---|---|
| 1 | | | | | |
| 2 | | | | | |
| 3 | | | | | |

---

## Kabul özeti

| Kategori | Toplam | Geçti | Kaldı | Bloklu |
|---|---|---|---|---|
| Grafikler | 6 | | | |
| Harita | 3 | | | |
| Canlı güncelleme | 4 | | | |
| Tarih formatı | 2 | | | |
| Excel | 7 | | | |
| Boş/Dolu | 4 | | | |
| Sıcaklık doğruluğu | 1 | | | |

Kabul kararı: ☐ Kabul ☐ Şartlı kabul ☐ Red  
Onaylayan / tarih: `____________`
