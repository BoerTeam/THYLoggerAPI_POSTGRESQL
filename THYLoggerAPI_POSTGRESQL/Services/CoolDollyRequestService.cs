using System.Globalization;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using THYLoggerAPI_POSTGRESQL.Context;
using THYLoggerAPI_POSTGRESQL.DTOs;
using THYLoggerAPI_POSTGRESQL.Model;

namespace THYLoggerAPI_POSTGRESQL.Services
{
    public class CoolDollyRequestService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CoolDollyRequestService> _logger;

        public CoolDollyRequestService(ApplicationDbContext context, ILogger<CoolDollyRequestService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ------------------------------------------------------------------
        // Format kuralları (varsayım - gerekirse gevşetin)
        // ------------------------------------------------------------------
        private static readonly Regex FlightNoRegex = new(@"^[A-Z0-9]{2}\d{1,4}[A-Z]?$", RegexOptions.Compiled);   // TK0001
        private static readonly Regex StationRegex = new(@"^[A-Z]{3}$", RegexOptions.Compiled);                    // IST
        private static readonly Regex AwbRegex = new(@"^\d{11}$", RegexOptions.Compiled);                          // 23512345678
        private static readonly Regex UldRegex = new(@"^[A-Z]{3}\d{4,5}[A-Z0-9]{2,3}$", RegexOptions.Compiled);    // AKE12345TK

        // ==================================================================
        // BPM -> createCoolDollyRequest
        // ==================================================================
        public async Task<CoolDollyResponse> CreateAsync(CreateCoolDollyRequestDto dto)
        {
            // 1) Alan doğrulama / normalizasyon
            var operationType = ParseOperationType(dto.OperationType);
            var awbNumber = NormalizeAwb(dto.AwbNumber);
            var flightNumber = RequireMatch(dto.FlightNumber, "flightNumber", FlightNoRegex);
            var flightDate = ParseFlightDate(dto.FlightDate);
            var departureStation = RequireMatch(dto.DepartureStation, "departureStation", StationRegex);
            var arrivalStation = RequireMatch(dto.ArrivalStation, "arrivalStation", StationRegex);
            var sccCode = NormalizeScc(dto.SccCode);
            var uldNumber = NormalizeUld(dto.UldNumber);
            var dollyNo = NormalizeDollyNo(dto.CoolDollyNumber);

            _logger.LogInformation(
                "BPM talep kaydetme isteği alındı. Dolly: {DollyNo}, AWB: {Awb}, ULD: {Uld}, Uçuş: {Flight} {FlightDate}",
                dollyNo, awbNumber, uldNumber, flightNumber, flightDate);

            // 2) Dolly'yi bul (CD-001)
            var dolly = await FindDollyAsync(dollyNo);

            // 3) İdempotency: BPM timeout sonrası aynı isteği tekrar gönderebilir.
            var existing = await _context.DollyRequests.AsNoTracking().FirstOrDefaultAsync(r =>
                r.DollyId == dolly.Id &&
                r.Status == DollyRequestStatus.Open &&
                r.AwbNumber == awbNumber &&
                r.UldNumber == uldNumber);

            if (existing != null)
            {
                _logger.LogInformation("Aynı talep zaten kayıtlı. Ref: {Ref}", existing.ReferenceNumber);
                return CoolDollyResponse.Success("Talep zaten kayıtlı", existing.ReferenceNumber);
            }

            // 4) Müsaitlik (CD-002): pasif Dolly veya zaten açık talebi olan Dolly
            if (!dolly.IsActive)
            {
                _logger.LogWarning("Pasif Dolly'ye atama denendi. Dolly: {Dolly}", dolly.Name);
                throw CoolDollyException.DollyNotAvailable();
            }

            var hasOpenRequest = await _context.DollyRequests
                .AnyAsync(r => r.DollyId == dolly.Id && r.Status == DollyRequestStatus.Open);

            if (hasOpenRequest)
            {
                _logger.LogWarning("Dolly başka bir açık talepte. Dolly: {Dolly}", dolly.Name);
                throw CoolDollyException.DollyNotAvailable();
            }

            // 5) Kaydet
            var request = new DollyRequest
            {
                ReferenceNumber = GenerateReferenceNumber(),
                OperationType = operationType,
                AwbNumber = awbNumber,
                FlightNumber = flightNumber,
                FlightDate = flightDate,
                DepartureStation = departureStation,
                ArrivalStation = arrivalStation,
                SccCode = sccCode,
                UldNumber = uldNumber,
                DollyId = dolly.Id,
                Status = DollyRequestStatus.Open,
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                await _context.DollyRequests.AddAsync(request);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException
            {
                ConstraintName: DollyRequest.OpenPerDollyIndexName
            })
            {
                // Aynı anda iki istek aynı Dolly'yi almaya çalıştı; partial unique index kazananı belirledi.
                _logger.LogWarning("Eşzamanlı atama çakışması. Dolly: {Dolly}", dolly.Name);
                throw CoolDollyException.DollyNotAvailable();
            }

            _logger.LogInformation("Dolly talebi oluşturuldu. Dolly: {Dolly}, Ref: {Ref}", dolly.Name, request.ReferenceNumber);
            return CoolDollyResponse.Success("Talep oluşturuldu", request.ReferenceNumber);
        }

        // ==================================================================
        // BPM -> terminateCoolDollyRequest
        // ==================================================================
        public async Task<CoolDollyResponse> TerminateAsync(TerminateCoolDollyRequestDto dto)
        {
            var operationType = ParseOperationType(dto.OperationType);
            var referenceNo = RequireText(dto.RequestReferenceNo, "requestReferenceNo");
            var flightNumber = RequireMatch(dto.FlightNumber, "flightNumber", FlightNoRegex);
            var flightDate = ParseFlightDate(dto.FlightDate);
            var dollyNo = NormalizeDollyNo(dto.CoolDollyNumber);

            _logger.LogInformation("BPM talep sonlandırma isteği alındı. Ref: {Ref}, Dolly: {DollyNo}", referenceNo, dollyNo);

            var request = await _context.DollyRequests
                .Include(r => r.Dolly)
                .FirstOrDefaultAsync(r => r.ReferenceNumber == referenceNo);

            if (request == null)
            {
                _logger.LogWarning("Sonlandırılacak talep bulunamadı. Ref: {Ref}", referenceNo);
                throw CoolDollyException.RequestNotFound();
            }

            // Gönderilen Dolly bu talebe ait olmalı
            if (!DollyMatches(request.Dolly!, dollyNo))
            {
                _logger.LogWarning("Terminate'te Dolly uyuşmadı. Ref: {Ref}, Gelen: {DollyNo}, Kayıtlı: {Dolly}",
                    referenceNo, dollyNo, request.Dolly!.Name);
                throw CoolDollyException.InvalidDolly();
            }

            // Uçuş / işlem tipi uyuşmazlığı reddedilmiyor, sadece loglanıyor (iş kuralı netleşince sıkılaştırılabilir)
            if (request.OperationType != operationType ||
                request.FlightNumber != flightNumber ||
                request.FlightDate != flightDate)
            {
                _logger.LogWarning(
                    "Terminate isteğindeki uçuş/işlem bilgisi talepten farklı. Ref: {Ref}", referenceNo);
            }

            // İdempotent: zaten kapalıysa başarılı say
            if (request.Status == DollyRequestStatus.Terminated)
            {
                return CoolDollyResponse.Success("Talep zaten sonlandırılmış", request.ReferenceNumber);
            }

            request.Status = DollyRequestStatus.Terminated;
            request.TerminatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Dolly boşaltıldı. Dolly: {Dolly}, Ref: {Ref}", request.Dolly!.Name, request.ReferenceNumber);
            return CoolDollyResponse.Success("Dolly boşaltıldı", request.ReferenceNumber);
        }

        // ==================================================================
        // Panel: liste ekranı
        // ==================================================================
        public async Task<PagedResult<DollyRequestListItem>> GetAllAsync(DollyRequestFilter f)
        {
            _logger.LogInformation("Dolly talep listesi sorgulanıyor.");

            var query = _context.DollyRequests.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(f.CoolDollyNo))
            {
                var no = f.CoolDollyNo.Trim().ToUpperInvariant();
                query = query.Where(r => r.Dolly!.Name.ToUpper().Contains(no) ||
                                         r.Dolly!.SerialNumber.ToUpper().Contains(no));
            }

            if (!string.IsNullOrWhiteSpace(f.AwbNo))
            {
                var awb = f.AwbNo.Replace("-", "").Replace(" ", "").Trim();
                query = query.Where(r => r.AwbNumber.Contains(awb));
            }

            if (!string.IsNullOrWhiteSpace(f.UldNo))
            {
                var uld = f.UldNo.Trim().ToUpperInvariant();
                query = query.Where(r => r.UldNumber.Contains(uld));
            }

            if (!string.IsNullOrWhiteSpace(f.FlightNo))
            {
                var flight = f.FlightNo.Trim().ToUpperInvariant();
                query = query.Where(r => r.FlightNumber == flight);
            }

            if (f.FlightDateFrom.HasValue) query = query.Where(r => r.FlightDate >= f.FlightDateFrom.Value);
            if (f.FlightDateTo.HasValue) query = query.Where(r => r.FlightDate <= f.FlightDateTo.Value);
            if (f.OperationType.HasValue) query = query.Where(r => r.OperationType == f.OperationType.Value);
            if (f.Status.HasValue) query = query.Where(r => r.Status == f.Status.Value);

            var page = f.Page < 1 ? 1 : f.Page;
            var pageSize = Math.Clamp(f.PageSize, 1, 200);

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(r => r.CreatedAt)
                .ThenByDescending(r => r.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(ToListItem)
                .ToListAsync();

            return new PagedResult<DollyRequestListItem>
            {
                Items = items,
                Total = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<DollyRequestListItem?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Dolly talebi aranıyor. ID: {Id}", id);

            var item = await _context.DollyRequests.AsNoTracking()
                .Where(r => r.Id == id)
                .Select(ToListItem)
                .FirstOrDefaultAsync();

            if (item == null)
            {
                _logger.LogWarning("Dolly talebi bulunamadı. ID: {Id}", id);
            }

            return item;
        }

        /// <summary>Bir Dolly'nin içinde şu an bulunan (açık) ULD/AWB kayıtları.</summary>
        public async Task<List<DollyRequestListItem>> GetOpenByDollyAsync(int dollyId)
        {
            _logger.LogInformation("Dolly içindeki açık talepler listeleniyor. DollyId: {DollyId}", dollyId);

            return await _context.DollyRequests.AsNoTracking()
                .Where(r => r.DollyId == dollyId && r.Status == DollyRequestStatus.Open)
                .OrderByDescending(r => r.CreatedAt)
                .Select(ToListItem)
                .ToListAsync();
        }

        /// <summary>Tüm Dolly'lerin içinde şu an bulunan (açık) kayıtlar. Ana ekran kartlarındaki ULD No / SCC için.</summary>
        public async Task<List<DollyRequestListItem>> GetOpenAllAsync()
        {
            _logger.LogInformation("Tüm açık Dolly talepleri listeleniyor.");

            return await _context.DollyRequests.AsNoTracking()
                .Where(r => r.Status == DollyRequestStatus.Open)
                .OrderBy(r => r.DollyId)
                .Select(ToListItem)
                .ToListAsync();
        }

        // Tek yerde tanımlı projeksiyon: liste, detay ve Dolly bazlı sorgu aynı alanları döner.
        private static readonly Expression<Func<DollyRequest, DollyRequestListItem>> ToListItem = r =>
            new DollyRequestListItem
            {
                Id = r.Id,
                ReferenceNumber = r.ReferenceNumber,
                DollyId = r.DollyId,
                CoolDollyName = r.Dolly!.Name,
                CoolDollySerialNumber = r.Dolly!.SerialNumber,
                AwbNo = r.AwbNumber,
                OperationType = r.OperationType,
                FlightNo = r.FlightNumber,
                FlightDate = r.FlightDate,
                DepartureStation = r.DepartureStation,
                ArrivalStation = r.ArrivalStation,
                SccCode = r.SccCode,
                UldNo = r.UldNumber,
                Status = r.Status,
                CreatedAt = r.CreatedAt,
                TerminatedAt = r.TerminatedAt
            };

        // ==================================================================
        // Yardımcılar
        // ==================================================================

        // BPM'de "Cool Dolly No" elle yazıldığı için büyük/küçük harf ve boşluk toleransı var.
        // Eşleşme Dolly.Name VEYA Dolly.SerialNumber üzerinden yapılır.
        private async Task<Dolly> FindDollyAsync(string dollyNo)
        {
            var dolly = await _context.Dolly.FirstOrDefaultAsync(d =>
                d.Name.ToUpper() == dollyNo || d.SerialNumber.ToUpper() == dollyNo);

            if (dolly == null)
            {
                _logger.LogWarning("Cool Dolly bulunamadı. Gelen no: {DollyNo}", dollyNo);
                throw CoolDollyException.InvalidDolly();
            }

            return dolly;
        }

        private static bool DollyMatches(Dolly dolly, string dollyNo) =>
            string.Equals(dolly.Name, dollyNo, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(dolly.SerialNumber, dollyNo, StringComparison.OrdinalIgnoreCase);

        private static string GenerateReferenceNumber() =>
            $"CD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";

        private static DollyOperationType ParseOperationType(string? value)
        {
            return value?.Trim().ToUpperInvariant() switch
            {
                "INBOUND" => DollyOperationType.INBOUND,
                "OUTBOUND" => DollyOperationType.OUTBOUND,
                _ => throw CoolDollyException.InvalidOperation()
            };
        }

        private static string RequireText(string? value, string field)
        {
            var v = value?.Trim();
            if (string.IsNullOrEmpty(v))
                throw CoolDollyException.InvalidRequest($"{field} zorunludur");
            return v;
        }

        private static string RequireMatch(string? value, string field, Regex regex)
        {
            var v = RequireText(value, field).ToUpperInvariant();
            if (!regex.IsMatch(v))
                throw CoolDollyException.InvalidRequest($"{field} formatı geçersiz");
            return v;
        }

        private static string NormalizeAwb(string? value)
        {
            // "235-12345678" ve "235 12345678" gelirse de kabul et
            var v = RequireText(value, "awbNumber").Replace("-", "").Replace(" ", "");
            if (!AwbRegex.IsMatch(v))
                throw CoolDollyException.InvalidRequest("awbNumber 11 haneli olmalıdır");
            return v;
        }

        private static DateOnly ParseFlightDate(string? value)
        {
            var v = RequireText(value, "flightDate");
            if (!DateOnly.TryParseExact(v, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                throw CoolDollyException.InvalidRequest("flightDate yyyy-MM-dd formatında olmalıdır");
            return date;
        }

        private static string? NormalizeScc(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            var v = value.Trim().ToUpperInvariant();
            if (v.Length > 50)
                throw CoolDollyException.InvalidRequest("sccCode en fazla 50 karakter olabilir");
            return v;
        }

        private static string NormalizeUld(string? value)
        {
            var v = value?.Trim().Replace(" ", "").ToUpperInvariant();
            if (string.IsNullOrEmpty(v) || !UldRegex.IsMatch(v))
                throw CoolDollyException.InvalidUld();
            return v;
        }

        private static string NormalizeDollyNo(string? value)
        {
            var v = value?.Trim().ToUpperInvariant();
            if (string.IsNullOrEmpty(v) || v.Length > 20)
                throw CoolDollyException.InvalidDolly();
            return v;
        }
    }
}
