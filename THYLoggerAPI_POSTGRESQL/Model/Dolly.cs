using System.ComponentModel.DataAnnotations;

namespace THYLoggerAPI_POSTGRESQL.Model
{
    public class Dolly
    {
        [Key]
        public int Id { get; set; }
        public string SerialNumber { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }

    }
}
