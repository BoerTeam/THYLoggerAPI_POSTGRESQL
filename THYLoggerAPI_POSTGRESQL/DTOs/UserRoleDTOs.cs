namespace THYLoggerAPI_POSTGRESQL.DTOs
{
    public class UserListDto
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public List<string> Roles { get; set; } = new();
    }

    public class AssignRoleDto
    {
        public int UserId { get; set; }
        public List<int> RoleIds { get; set; } = new();
    }

    public class CreateRoleDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<int> PermissionIds { get; set; } = new();
    }
}