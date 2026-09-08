using Microsoft.EntityFrameworkCore;
using THYLoggerAPI_POSTGRESQL.Context;
using THYLoggerAPI_POSTGRESQL.DTOs;
using THYLoggerAPI_POSTGRESQL.Model;

namespace THYLoggerAPI_POSTGRESQL.Services
{
    public class UserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Otomatik kaydolmuş tüm kullanıcıları listeler
        public async Task<List<UserListDto>> GetAllUsersAsync()
        {
            return await _context.Users
                .Select(u => new UserListDto
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    IsActive = u.IsActive,
                    Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList()
                })
                .ToListAsync();
        }

        // Seçilen kullanıcıya roller atar
        public async Task<bool> AssignRolesToUserAsync(AssignRoleDto dto)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.Id == dto.UserId);

            if (user == null) return false;

            // Mevcut rolleri temizle
            _context.UserRoles.RemoveRange(user.UserRoles);

            // Yeni seçilen rolleri ekle
            foreach (var roleId in dto.RoleIds)
            {
                _context.UserRoles.Add(new UserRole
                {
                    UserId = dto.UserId,
                    RoleId = roleId
                });
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}