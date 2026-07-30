namespace Dashboard.DTO
{
    public class Sicaklik
    {
        public int Id { get; set; }

        public DateTime? Time { get; set; }

        public float? Sicaklik1 { get; set; }

        public int DollyId { get; set; }

        public string SerialNumber { get; set; }
       
        public Dolly? Dolly { get; set; }
    }
}
