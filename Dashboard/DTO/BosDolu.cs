namespace Dashboard.DTO
{
    public class BosDolu
    {
        public int Id { get; set; }

        public DateTime? Time { get; set; }

        public bool? SensorDegeri { get; set; }

        public string? LoggerId { get; set; }

        // Veritabanı ilişkisi için gerekli olan ID
        public int DollyId { get; set; }

        // JSON'dan gelecek olan seri numarası (Veritabanına kaydedilmez)

        public string SerialNumber { get; set; }

        public Dolly? Dolly { get; set; }
    }
}
