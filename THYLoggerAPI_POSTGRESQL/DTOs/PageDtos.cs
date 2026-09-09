namespace THYLoggerAPI_POSTGRESQL.DTOs
{
    public class PageListDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Route { get; set; } = string.Empty;
        public string PermissionCode { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public int Order { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreatePageDto
    {
        public string Name { get; set; } = string.Empty;
        public string Route { get; set; } = string.Empty;
        public string PermissionCode { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public int Order { get; set; }
    }
}