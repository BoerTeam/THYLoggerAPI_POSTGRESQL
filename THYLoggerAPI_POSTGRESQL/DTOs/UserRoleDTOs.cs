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

    public class UserRoleDetailDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public List<RoleItemDto> Roles { get; set; } = new();
    }

    public class RoleItemDto
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public bool IsAssigned { get; set; }
    }

    // --- RoleService İçin Eksik Olan DTO Tanımları ---

    public class RoleDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public List<RolePermissionDto> RolePermissions { get; set; } = new();
    }

    public class RolePermissionDto
    {
        public int RoleId { get; set; }
        public int PermissionId { get; set; }
        public PermissionDto? Permission { get; set; }
    }

    public class PermissionDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }
}