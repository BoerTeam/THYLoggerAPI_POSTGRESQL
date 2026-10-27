# Cool Dolly – Kullanıcı Kabul Testi (UAT)

Kaynak: ilk kurulum sonrası müşteri tespitleri (`cool dolly tespit.xlsx`) ve GitHub issue'ları.
Test edilen sürüm: **v1.1.0**

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
| G-03 | #11 | Başlangıç ve bitiş alanlarına tıkla; takvimden tarih ve saat seç. Takvimin altındaki **Tamam**'a (veya takvimin dışına) tıkla, sonra **Uygula**'ya tıkla. | Takvim alanın hemen altında açılır, Tamam / dışına tıklayınca kapanır. Grafikler seçilen aralığı gösterir. 24 saatten uzun aralıkta etiketlerde `gg/aa SS:dd` görünür. | | |
| G-04 | #11 | Bitişi başlangıçtan önce seçip Uygula'ya tıkla. | "Geçerli bir aralık seçin" uyarısı çıkar, grafik bozulmaz. | | |
| G-05 | #12 | Nem grafiğine bak (değer değişimi olan bir aralıkta). | Y ekseni sabit **0–100** (40–110 gibi kaymaz); çizgi gerçek değişimi gösterir, nokta işaretleri çizgiyi kalınlaştırmaz. | | |
| G-06 | #13 | Sıcaklık grafiğine bak. | Y ekseni en az **0–25** görünür (4–22 gibi kaymaz); çizgi değişir. Değer 25'i aşarsa eksen genişler. | | |

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
| E-01 | #18 | Excel formundaki başlangıç/bitiş takvimlerinden son 1 saatlik aralık seç (takvim Tamam ile kapanmalı) ve indir. | Yalnızca seçilen aralıktaki kayıtlar gelir (ilk/son satır saatlerini kontrol et). | | |
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

## 7. Denetim Kayıtları (Audit Log)

Ön koşul: Admin rolüne sahip kullanıcı ve Admin olmayan bir test kullanıcısı.

| ID | Issue | Adımlar | Beklenen sonuç | Sonuç | Not |
|---|---|---|---|---|---|
| A-01 | – | Admin ile giriş yap, sol menüye bak. | **Denetim Kayıtları** menü öğesi görünür; tıklayınca sayfa açılır. | | |
| A-02 | – | Admin olmayan kullanıcı ile giriş yap. | Menüde Denetim Kayıtları görünmez; `/AuditLog/Index` adresi doğrudan açılırsa "Erişim Reddedildi" sayfasına yönlenir. | | |
| A-03 | – | Menüden sayfayı **hiçbir filtre seçmeden** aç. | Tablo boş gelmez; kayıtlar en yeniden eskiye listelenir (açılışta gizli bir filtre uygulanmamalı); tarihler Türkiye saatinde `gg.aa.yyyy SS:dd:ss` formatındadır; toplam sayı görünür. | | |
| A-04 | – | **Varlık** ve **İşlem** listelerinden seçim yapıp Filtrele'ye tıkla. | Yalnızca seçilen varlık/işlem kayıtları gelir (örn. Varlık: Dolly, İşlem: Modified). Seçilen işlem Filtrele sonrası listede seçili kalır. | | |
| A-05 | – | Başlangıç ve bitiş tarihi seç (takvim `gg/aa/yyyy`), Filtrele. | Yalnızca seçilen aralıktaki kayıtlar gelir; ilk/son satırın saatini kontrol et. | | |
| A-06 | – | Bir dolly adını düzenle ve kaydet; ardından Varlık: Dolly, İşlem: Modified filtrele. | Yeni bir kayıt oluşur; **Değerleri göster** ile eski ve yeni değer görünür. | | |
| A-07 | – | **Değerlerde ara** alanına bir değer (örn. dolly adı) yaz. | Eski/yeni değerlerinde bu metin geçen kayıtlar gelir. | | |
| A-08 | – | Kayıt No veya Kullanıcı alanı ile filtrele. | Yalnızca eşleşen kayıtlar gelir. *(Kullanıcı sütunu şu an kayıtlarda boş olabilir; bilinen sınırlama.)* | | |
| A-09 | – | Bir işlem filtresi (örn. Added) seçip satır sayısını 25 yap; Sonraki / Önceki ile gez. | Sayfalama çalışır, işlem dahil uygulanan tüm filtreler sayfa değişince korunur. | | |
| A-10 | – | Hiç eşleşmeyen bir filtre uygula. | "Kayıt bulunamadı." mesajı görünür, hata oluşmaz. | | |
| A-11 | – | **Temizle**'ye tıkla. | Filtreler sıfırlanır, tüm kayıtlar listelenir. | | |
| A-12 | – | Cihazdan yeni bir sıcaklık/nem verisi gönder (veya bir dolly ekle) ve sayfayı yenile. | Yeni işlem Varlık: Sicaklik / Nem / Dolly, İşlem: Added olarak en üstte görünür; Yeni değer detayında gönderilen değerler yer alır. | | |

## 8. Sıcaklık doğruluğu

| ID | Issue | Adımlar | Beklenen sonuç | Sonuç | Not |
|---|---|---|---|---|---|
| S-01 | #24 | Dolly üzerindeki gösterge ile dashboard'daki sıcaklığı **aynı anda** karşılaştır; en az 3 farklı zamanda tekrarla. Gösterge, sistem değeri ve (varsa) ham mA değerini kaydet. | Fark müşterinin kabul ettiği toleransta (belirlenecek: `____ °C`). Fark sistematikse sensör aralığını (4 mA ve 20 mA kaç °C) bildir; kalibrasyon ayarı güncellenir. | | |

| Ölçüm | Saat | Dolly göstergesi (°C) | Sistem (°C) | Ham mA | Fark |
|---|---|---|---|---|---|
| 1 | | | | | |
| 2 | | | | | |
| 3 | | | | | |

## 9. Cihaz Konfigürasyonu (uzaktan config)

Ön koşul: Admin kullanıcı; SICK TDC cihazı `Config/Get` çağırabiliyor (API adresi cihazda tanımlı). Admin olmayan bir test kullanıcısı.

| ID | Issue | Adımlar | Beklenen sonuç | Sonuç | Not |
|---|---|---|---|---|---|
| K-01 | – | Admin ile giriş yap, menüye bak. | **Cihaz Konfigürasyonu** ve **Konfigürasyon Raporu** menü öğeleri görünür. Admin olmayan kullanıcıda görünmez; adresler doğrudan açılırsa "Erişim Reddedildi" sayfasına yönlenir. | | |
| K-02 | – | Cihaz Konfigürasyonu sayfasını aç. | Tüm dolly'ler tabloda listelenir (ad, seri no, model, uygulama sürümü, sunucu/uygulanan cfg, durum). Form varsayılan değerlerle (300/1/60/10/60 sn, 3 kanal, 4 endpoint) dolu gelir. | | |
| K-03 | – | Cihazı hiç seçmeden **Seçili cihazlara gönder**'e tıkla. | "En az bir cihaz seçmelisiniz" uyarısı çıkar, istek gitmez. | | |
| K-04 | – | **Tümünü seç** / **Temizle** ve arama kutusunu dene. | Seçili sayısı doğru güncellenir; arama yalnızca görünen satırları etkiler. | | |
| K-05 | – | Cihazı (SICK) açıp `Config/Get`'i çağırmasını bekle (configVersion 0 ile). | Cihaz kaydı oluşur: model ve uygulama sürümü sayfada görünür; cihaz varsayılan config'i **sürüm 1** ile alır. | | |
| K-06 | – | **Modeli seç** ile bir cihaz modeli seç; sayıyı kontrol et. | Yalnızca o modeldeki cihazlar işaretlenir. | | |
| K-07 | – | Bir cihazı tek tek işaretle, bir değeri değiştir (örn. gönderim aralığı 30), not yaz ve gönder; onay penceresinde Tamam. | "Konfigürasyon N cihaz için yayınlandı" mesajı çıkar. Cihazın sunucu cfg sürümü 1 artar, durumu **Bekliyor** olur. | | |
| K-08 | – | Geçersiz değer ver (süre 0, endpoint `/` ile başlamıyor). | Form gönderilmez veya "yayınlanamadı" uyarısı çıkar; sürüm artmaz. | | |
| K-09 | – | Analog kanal ekle, birini sil, gönder. | Cihaza giden config'te kanal listesi ekranla aynıdır (sıra ve alanlar doğru). | | |
| K-10 | – | Cihazın gece saatinde `Config/Get`'i çağırmasını bekle (veya elle çağır). | Yeni config yanıtta gelir (`"Konfigürasyon gönderildi"`); Rapor'da olay **Çekildi** görünür. Cihaz uygulayıp yeniden başlar. | | |
| K-11 | – | Cihaz yeniden başlayıp `Config/GetVsControl` ile bildirimde bulunsun. | Rapor'da cihaz durumu **Uygulandı**, uygulanan cfg = sunucu cfg; olay **Uygulandı** zaman damgasıyla görünür. | | |
| K-12 | – | Aynı sürümle `Config/Get`'i tekrar çağır. | Yanıt `"Değişiklik yok"`, `config: null`. Rapor'a yeni "Çekildi" olayı eklenmez. | | |
| K-13 | – | Cihazdan `GetVsControl`'ü `success=false&errorMessage=...` ile çağır. | Rapor'da cihaz **Hata** durumunda, hata metni ve zamanı görünür; hatalı cihazlar tabloda en üstte. Yeni config yayınlanınca hata temizlenir. | | |
| K-14 | – | Rapor'da cihaz / olay türü / tarih filtreleri ile satır sayısını dene, sayfalarda gez. | Yalnızca eşleşen olaylar gelir; filtreler sayfa değişince korunur; Temizle sıfırlar. Tarihler Türkiye saatindedir. | | |
| K-15 | – | Var olmayan seri no ile `Config/Get` çağır; `serialNumber` göndermeden de dene. | Var olmayan seri no için 404, eksik alan için 400 döner. | | |
| K-16 | – | `Config/DollyLog` ile multipart log dosyası yükle. | Başarılı yanıt döner; dosya sunucuda `Logs/DollyLogs/{seriNo}/` altına kaydedilir; Rapor'da **Log yüklendi** olayı görünür. | | |
| K-17 | – | Config yayını yapıp Denetim Kayıtları'na bak. | Cihaz telemetri güncellemeleri (son görülme vb.) denetim kayıtlarını doldurmaz. | | |

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
| Denetim Kayıtları | 12 | | | |
| Sıcaklık doğruluğu | 1 | | | |
| Cihaz Konfigürasyonu | 17 | | | |

Kabul kararı: ☐ Kabul ☐ Şartlı kabul ☐ Red  
Onaylayan / tarih: `____________`
