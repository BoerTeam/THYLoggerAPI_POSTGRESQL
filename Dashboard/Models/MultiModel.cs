using Dashboard.DTO;


namespace Dashboard.Models
{
    public class MultiModel
    {
        public List<Sicaklik> Sicaklik { get; set; }
        public List<Nem> nem { get; set; }
        public List<BosDolu> bosDolu { get; set; }
        public List<Gpsdatum> gpsdatum { get; set; }
        public List<Dolly> DollyList { get; set; }
    }
}
