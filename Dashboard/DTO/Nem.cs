namespace Dashboard.DTO
{
    public class Nem
    {
        public int Id { get; set; }
        public DateTime? Time { get; set; }
        public float? Nem1 { get; set; }

        // Veritabanında saklanan zorunlu alan
        public int DollyId { get; set; }

        // JSON'dan gelecek olan seri numarası (Veritabanına kaydedilmez)

        public string SerialNumber { get; set; }

        
        public Dolly? Dolly { get; set; }
    }
}
