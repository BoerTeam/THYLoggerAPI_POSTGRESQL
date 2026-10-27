namespace Dashboard.DTO
{
    // Config ekranlarında durum / olay etiketleri
    public static class ConfigStatusText
    {
        public static string Label(string status) => status switch
        {
            "Applied" => "Uygulandı",
            "Fetched" => "Çekildi (uygulama bekleniyor)",
            "Pending" => "Bekliyor",
            "Error" => "Hata",
            "NeverSeen" => "Hiç bağlanmadı",
            "NoConfig" => "Config yok",
            _ => status
        };

        public static string Badge(string status) => status switch
        {
            "Applied" => "bg-success",
            "Fetched" => "bg-info",
            "Pending" => "bg-warning text-dark",
            "Error" => "bg-danger",
            _ => "bg-secondary"
        };

        public static string EventLabel(string type) => type switch
        {
            "Published" => "Yayınlandı",
            "Fetched" => "Çekildi",
            "Applied" => "Uygulandı",
            "Error" => "Hata",
            "LogUploaded" => "Log yüklendi",
            _ => type
        };

        public static string EventBadge(string type) => type switch
        {
            "Published" => "bg-primary",
            "Fetched" => "bg-info",
            "Applied" => "bg-success",
            "Error" => "bg-danger",
            _ => "bg-secondary"
        };
    }
}
