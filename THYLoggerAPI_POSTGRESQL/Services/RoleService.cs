using Microsoft.EntityFrameworkCore;
using THYLoggerAPI_POSTGRESQL.Context;
using THYLoggerAPI_POSTGRESQL.DTOs;
using THYLoggerAPI_POSTGRESQL.Model;

namespace THYLoggerAPI_POSTGRESQL.Services
{
    public class RoleService
    {
        private readonly ApplicationDbContext _context;

        public RoleService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Tüm Rolleri Getirir
        public async Task<List<Role>> GetAllRolesAsync()
        {
            return await _context.Roles
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .ToListAsync();
        }

        // Tüm İzinleri (Permissions) Getirir (Arayüzde checkbox olarak göstermek için)
        public async Task<List<Permission>> GetAllPermissionsAsync()
        {
            return await _context.Permissions.Where(p => p.IsActive).ToListAsync();
        }

        // Yeni Rol Oluşturur ve İzinlerini Bağlar
        public async Task<bool> CreateRoleAsync(CreateRoleDto dto)
        {
            var role = new Role
            {
                Name = dto.Name,
                Description = dto.Description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            if (dto.PermissionIds.Any())
            {
                foreach (var permId in dto.PermissionIds)
                {
                    _context.RolePermissions.Add(new RolePermission
                    {
                        RoleId = role.Id,
                        PermissionId = permId
                    });
                }
                await _context.SaveChangesAsync();
            }

            return true;
        }
    }
}