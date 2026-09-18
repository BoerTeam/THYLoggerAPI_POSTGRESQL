namespace Dashboard.DTO
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
}