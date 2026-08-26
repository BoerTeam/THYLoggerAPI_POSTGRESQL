namespace Dashboard.Models
{
    public class LdapAuthenticationOptions
    {
        public string? Host { get; set; }
        public int Port { get; set; } = 389;
        public string? Domain { get; set; }
        public string? BaseDn { get; set; }
        public string SearchFilter { get; set; } = "(sAMAccountName={0})";
        public bool UseSsl { get; set; }
    }
}
