# Cool Dolly – Cihaz İmaj Güncelleme Testi

Kapsam: PR #26 – `UploadImage`, `DownloadImage` ve `Config/Get` yanıtındaki `getLastImageName` alanı.
Branch: `feature/upload-image`

**Özet:** Dashboard/yönetici bir imaj `.tar` dosyasını API'ye yükler. Cihaz her `Config/Get` çağrısında yanıttaki `getLastImageName` alanından en güncel imajın adını görür ve `DownloadImage` ile indirip kurar. *(İmajı cihazda kurma adımı cihaz uygulamasının işidir; bu doküman yalnızca API tarafını test eder.)*

**Ön koşullar**
- API, `feature/upload-image` branch'iyle çalışıyor (`dotnet run`, Swagger: `http://localhost:5062/swagger`).
- Veritabanı erişilebilir; en az 1 dolly tanımlı. Dolly seri numarası: `____________`
- Test için 2 adet küçük `.tar` dosyası hazır (içeriği önemsiz, örn. 1 MB): `imaj1.tar`, `imaj2.tar`. Ayrıca bir `.txt` dosyası (`test.txt`).
- İlk koşuda `THYLoggerAPI_POSTGRESQL/Images/` dizini **boş veya yok** olmalı (yoksa A-01'i atla/Not'a yaz).
- Test tarihi: `____________`  Tester: `____________`

**Sonuç değerleri:** ✅ Geçti · ❌ Kaldı · ⏸ Bloklu / uygulanamaz

**Örnek komutlar** (`<SN>` = dolly seri numarası, `<AD>` = yüklenen dosya adı)
```
# Yükleme
curl -X POST http://localhost:5062/api/Config/UploadImage -F "ImageFile=@imaj1.tar"

# Config al (son imaj adı yanıtta)
curl -X POST http://localhost:5062/api/Config/Get -H "Content-Type: application/json" \
  -d '{"serialNumber":"<SN>","deviceType":"test","appVersion":"1.0","configVersion":1}'

# İndirme
curl -o indirilen.tar "http://localhost:5062/api/Config/DownloadImage?fileName=<AD>"
```

---

## 1. İmaj yükleme (UploadImage)

| ID | Adımlar | Beklenen sonuç | Sonuç | Not |
|---|---|---|---|---|
| U-01 | `imaj1.tar` dosyasını `ImageFile` alanıyla yükle. | **200**. Yanıt: `message` ve `fileName` (`yyyyMMdd_HHmmss_imaj1.tar` biçiminde). Dosya `Images/` dizininde görünür. | | |
| U-02 | `test.txt` dosyasını yükle. | **400**, ".tar dosyası olmalıdır" mesajı. `Images/` içine dosya eklenmez. | | |
| U-03 | `ImageFile` alanı olmadan (boş form) istek at. | **400**, "ImageFile zorunludur". | | |
| U-04 | 0 baytlık `.tar` dosyası yükle. | **400**, "ImageFile zorunludur". | | |
| U-05 | `imaj1.tar` dosyasını art arda iki kez yükle. | İki ayrı dosya oluşur (farklı zaman damgası); biri diğerinin üstüne yazılmaz. | | |
| U-06 | Dosya adı `..\..\evil.tar` olan bir dosya yükle (curl: `-F "ImageFile=@imaj1.tar;filename=..\..\evil.tar"`). | Dosya yalnızca `Images/` içine, temizlenmiş adla (`..._evil.tar`) kaydedilir; dizin dışına yazılmaz. | | |
| U-07 | Büyük (≈ 200 MB+) bir `.tar` yükle. | Başarıyla yüklenir (sınır 2 GB), süre Not'a yazılır. Yükleme sırasında `Images/` içinde `.part` dosyası görünür, bitince kaybolur. | | |

## 2. Son imaj adı (getLastImageName)

| ID | Adımlar | Beklenen sonuç | Sonuç | Not |
|---|---|---|---|---|
| A-01 | `Images/` boşken `Config/Get` çağır (`configVersion` = sunucudaki sürümden farklı). | **200**, config gelir; `getLastImageName` = `null`. | | |
| A-02 | `imaj1.tar` yükle, ardından `Config/Get` çağır. | `getLastImageName` = yüklenen dosyanın adı (U-01'deki `fileName`). | | |
| A-03 | `configVersion` değerini sunucudakiyle **aynı** gönder ("Değişiklik yok" yanıtı). | `message` = "Değişiklik yok", `config` = null, ama `getLastImageName` **yine dolu** gelir. | | |
| A-04 | `imaj2.tar` yükle, `Config/Get` çağır. | `getLastImageName` artık `imaj2.tar`'ın adıdır (en son yüklenen). | | |
| A-05 | Yanıtın JSON alan adını kontrol et. | Alan adı `getLastImageName` (camelCase). | | |
| A-06 | Var olmayan seri numarasıyla `Config/Get` çağır. | **404** (mevcut davranış bozulmamış). | | |

## 3. İmaj indirme (DownloadImage)

| ID | Adımlar | Beklenen sonuç | Sonuç | Not |
|---|---|---|---|---|
| D-01 | A-04'teki `getLastImageName` değeriyle `DownloadImage` çağır. | **200**, dosya iner. Boyutu ve SHA-256 hash'i yüklenen `imaj2.tar` ile aynıdır (`certutil -hashfile <dosya> SHA256`). | | |
| D-02 | Daha eski bir imajın adıyla indir (U-01'deki ad). | **200**, doğru dosya iner. | | |
| D-03 | Var olmayan bir dosya adıyla çağır. | **404**, "İmaj dosyası bulunamadı". | | |
| D-04 | `fileName` boş / hiç verilmeden çağır. | **404**. | | |
| D-05 | `fileName=..%2FProgram.cs` ve `fileName=..\appsettings.json` ile çağır. | **404**; proje dosyaları indirilemez. | | |
| D-06 | `Images/` içine elle bir `x.txt` koy, `fileName=x.txt` ile çağır. | **404** (yalnızca `.tar` indirilebilir). | | |
| D-07 | Büyük dosyayı `curl -C - -o b.tar "<url>"` ile yarıda kes (Ctrl+C), tekrar çalıştırarak devam ettir. | İndirme kaldığı yerden devam eder; sonuç dosyasının hash'i doğrudur (range desteği). | | |

## 4. Uçtan uca ve regresyon

| ID | Adımlar | Beklenen sonuç | Sonuç | Not |
|---|---|---|---|---|
| E-01 | Gerçek cihazda (veya cihaz simülatöründe): imaj yükle → cihazın `Config/Get` çağrısında `getLastImageName`'i al → `DownloadImage` ile indir → kur. | Cihaz yeni imajı indirir ve kurar. *(Cihaz uygulaması hazır değilse ⏸.)* | | |
| E-02 | Mevcut config akışı: dashboard'dan config yayınla, cihaz `Config/Get` ile çeksin, `GetVsControl` ile bildirsin. | Önceki sürümdeki gibi çalışır; ek alan (`getLastImageName`) akışı bozmaz. | | |
| E-03 | `Config/DollyLog` ile log dosyası yükle. | Eskisi gibi çalışır (**200**). | | |
| E-04 | API'yi yeniden başlat, `Config/Get` çağır. | `getLastImageName` hâlâ en son yüklenen imajı verir (bilgi dosya sisteminde tutulur). | | |

---

## Bilinen notlar
- `UploadImage` ve `DownloadImage`, diğer `Config` uçları gibi **kimlik doğrulamasızdır**; yetkilendirme (API key vb.) ayrıca değerlendirilecek.
- "Son imaj", `Images/` içindeki `.tar` dosyalarından **dosya tarihi en yeni olan**dır. Dosyayı elle kopyalayıp tarihini değiştirmek sonucu etkiler.
- Eski imajlar otomatik silinmez; `Images/` dizini disk dolmaması için periyodik temizlenmelidir.

## Kabul özeti

| Bölüm | Toplam | Geçti | Kaldı | Bloklu |
|---|---|---|---|---|
| 1. İmaj yükleme | 7 | | | |
| 2. Son imaj adı | 6 | | | |
| 3. İmaj indirme | 7 | | | |
| 4. Uçtan uca ve regresyon | 4 | | | |

Genel sonuç: ☐ Kabul ☐ Şartlı kabul ☐ Red   Onaylayan: `____________`  Tarih: `____________`
