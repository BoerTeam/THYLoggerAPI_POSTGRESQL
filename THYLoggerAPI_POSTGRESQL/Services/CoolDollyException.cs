namespace THYLoggerAPI_POSTGRESQL.Services
{
    /// <summary>
    /// Servis mapping dokümanı v1.1 hata kodlarını (CD-001, CD-003, CD-004, CD-005) ve HTTP status bilgisini taşır.
    /// CD-002 (Dolly müsaitlik kontrolü) v1.1 ile kapsam dışı bırakılmıştır.
    /// </summary>
    public class CoolDollyException : Exception
    {
        public string Code { get; }
        public int StatusCode { get; }

        public CoolDollyException(string code, string message, int statusCode = StatusCodes.Status400BadRequest)
            : base(message)
        {
            Code = code;
            StatusCode = statusCode;
        }

        // --- Doküman v1.1 Bölüm 7 Kodları ---
        public static CoolDollyException InvalidDolly() =>
            new("CD-001", "Geçersiz Cool Dolly numarası", StatusCodes.Status400BadRequest);

        public static CoolDollyException UldNotAvailable(string? detail = null) =>
            new("CD-003", string.IsNullOrWhiteSpace(detail) ? "ULD müsait değil" : detail, StatusCodes.Status409Conflict);

        public static CoolDollyException SystemError() =>
            new("CD-004", "Sistem hatası", StatusCodes.Status500InternalServerError);

        public static CoolDollyException InvalidOperation() =>
            new("CD-005", "Geçersiz işlem tipi", StatusCodes.Status400BadRequest);

        // --- Alan doğrulama / yardımcı fabrikalar (Doküman kodlarına map'lenir) ---
        public static CoolDollyException InvalidUld() =>
            new("CD-005", "Geçersiz ULD numarası", StatusCodes.Status400BadRequest);

        public static CoolDollyException InvalidRequest(string detail) =>
            new("CD-005", $"Geçersiz istek: {detail}", StatusCodes.Status400BadRequest);

        public static CoolDollyException RequestNotFound() =>
            new("CD-003", "Eşleşen aktif talep bulunamadı", StatusCodes.Status404NotFound);

        public static CoolDollyException Unauthorized() =>
            new("AUTH-401", "Yetkisiz erişim: Basic Auth bilgileri eksik veya hatalı", StatusCodes.Status401Unauthorized);

        public static CoolDollyException RateLimited() =>
            new("RATE-429", "İstek limiti aşıldı (dakikada 100)", StatusCodes.Status429TooManyRequests);
    }
}