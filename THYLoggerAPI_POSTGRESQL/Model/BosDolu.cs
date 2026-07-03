using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace THYLoggerAPI_POSTGRESQL.Model
{
    public class BosDolu
    {
        [Key]
        public int Id { get; set; }

        public DateTime? Time { get; set; }

        public bool? SensorDegeri { get; set; }

        public string? LoggerId { get; set; }

        // Veritabanı ilişkisi için gerekli olan ID
        public int DollyId { get; set; }

        // JSON'dan gelecek olan seri numarası (Veritabanına kaydedilmez)
        
        public string SerialNumber { get; set; }

        [JsonIgnore]
        public Dolly? Dolly { get; set; }
    }
}
