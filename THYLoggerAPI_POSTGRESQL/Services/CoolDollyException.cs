namespace THYLoggerAPI_POSTGRESQL.Services
{
    /// <summary>
    /// Servis mapping dokümanındaki hata kodlarını (CD-001 .. CD-005) ve HTTP status'ünü taşır.
    /// CD-006 ve sonrası dokümanda YOKTU; BPM ekibiyle netleştirilip dokümana eklenmeli.
    /// </summary>
    public class CoolDollyException : Exception
    {
        public string Code { get; }
        public int StatusCode { get; }

        public CoolDollyException(string code, string message, int statusCode) : base(message)
        {
            Code = code;
            StatusCode = statusCode;
        }

        // --- Dokümandaki kodlar ---
        public static CoolDollyException InvalidDolly() =>
            new("CD-001", "Geçersiz Cool Dolly numarası", StatusCodes.Status400BadRequest);

        public static CoolDollyException DollyNotAvailable() =>
            new("CD-002", "Dolly şu anda müsait değil", StatusCodes.Status409Conflict);

        public static CoolDollyException InvalidUld() =>
            new("CD-003", "Geçersiz ULD numarası", StatusCodes.Status400BadRequest);

        public static CoolDollyException SystemError() =>
            new("CD-004", "Sistem hatası", StatusCodes.Status500InternalServerError);

        public static CoolDollyException InvalidOperation() =>
            new("CD-005", "Geçersiz işlem tipi", StatusCodes.Status400BadRequest);

        // --- Dokümanda olmayan, önerilen ek kodlar ---
        public static CoolDollyException InvalidRequest(string detail) =>
            new("CD-006", $"Geçersiz istek: {detail}", StatusCodes.Status400BadRequest);

        public static CoolDollyException RequestNotFound() =>
            new("CD-007", "Talep bulunamadı", StatusCodes.Status404NotFound);

        public static CoolDollyException Unauthorized() =>
            new("CD-008", "Yetkisiz erişim", StatusCodes.Status401Unauthorized);

        public static CoolDollyException RateLimited() =>
            new("CD-009", "İstek limiti aşıldı (dakikada 100)", StatusCodes.Status429TooManyRequests);
    }
}
