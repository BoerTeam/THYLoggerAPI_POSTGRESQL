using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace THYLoggerAPI_POSTGRESQL.Model
{
    public class Sicaklik
    {
        [Key]
        public int Id { get; set; }
        [Column(TypeName = "timestamp without time zone")]
        public DateTime? Time { get; set; }

        public float? Sicaklik1 { get; set; }

        public int DollyId { get; set; }
       
        public string SerialNumber { get; set; }

        [JsonIgnore]
        public Dolly? Dolly { get; set; }
    }
}
