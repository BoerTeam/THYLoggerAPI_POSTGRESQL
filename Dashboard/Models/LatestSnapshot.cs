using Dashboard.DTO;

namespace Dashboard.Models
{
    // API'nin api/Latest/GetAll cevabi: her dolly icin en son kayitlar
    public class LatestSnapshot
    {
        public List<Sicaklik> Sicaklik { get; set; } = new();
        public List<Nem> Nem { get; set; } = new();
        public List<Gpsdatum> Gps { get; set; } = new();
        public List<BosDolu> BosDolu { get; set; } = new();
    }
}
