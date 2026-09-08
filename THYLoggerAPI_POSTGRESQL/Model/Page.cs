namespace THYLoggerAPI_POSTGRESQL.Model
{
    public class Page
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Route { get; set; } = string.Empty;

        public string PermissionCode { get; set; } = string.Empty;

        public string? Icon { get; set; }

        public int Order { get; set; }

        public bool IsActive { get; set; } = true;
    }
}