using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace THYLoggerAPI_POSTGRESQL.Model
{
    public class Gpsdatum
    {
        [Key]
        public int Id { get; set; }

        public bool? IsEnabled { get; set; }
        public float? Latitude { get; set; }
        public float? Longitude { get; set; }
        [Column(TypeName = "timestamp without time zone")]
        public DateTime? Time { get; set; }
        public float? Altitude { get; set; }
        public float? SpeedKnots { get; set; }
        public float? SpeedMph { get; set; }
        public float? SpeedKmh { get; set; }
        public string? Course { get; set; }
        public int? Fix { get; set; }
        public string? FixAsString { get; set; }
        public int? NumberOfSatellites { get; set; }
        public bool? GpsFixAvailable { get; set; }
        public float? Hdop { get; set; }
        public int? QualityType { get; set; }

        // Veritabanı ilişkisi için gerekli ID
        public int DollyId { get; set; }

        // JSON'dan gelecek olan seri numarası (Veritabanına kaydedilmez)
        
        public string SerialNumber { get; set; }

        [JsonIgnore]
        public Dolly? Dolly { get; set; }
    }
}
