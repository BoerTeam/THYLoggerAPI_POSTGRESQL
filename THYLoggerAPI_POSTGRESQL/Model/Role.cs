namespace THYLoggerAPI_POSTGRESQL.Model
{
    public class Role
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsSystemRole { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public ICollection<UserRole> UserRoles { get; set; }
            = new List<UserRole>();

        public ICollection<RolePermission> RolePermissions { get; set; }
            = new List<RolePermission>();
    }
}