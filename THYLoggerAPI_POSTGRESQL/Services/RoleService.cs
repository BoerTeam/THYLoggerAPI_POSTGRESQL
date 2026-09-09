using Microsoft.EntityFrameworkCore;
using THYLoggerAPI_POSTGRESQL.Context;
using THYLoggerAPI_POSTGRESQL.DTOs;
using THYLoggerAPI_POSTGRESQL.Model;

namespace THYLoggerAPI_POSTGRESQL.Services
{
    public class RoleService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RoleService> _logger;

        public RoleService(ApplicationDbContext context, ILogger<RoleService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Tüm Rolleri Getirir
        public async Task<List<Role>> GetAllRolesAsync()
        {
            _logger.LogInformation("Tüm sistem rolleri listeleniyor.");

            try
            {
                var roles = await _context.Roles
                    .AsNoTracking()
                    .Include(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
                    .ToListAsync();

                _logger.LogInformation("Sistem rolleri başarıyla getirildi. Toplam Rol Sayısı: {RoleCount}", roles.Count);
                return roles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sistem rolleri çekilirken bir hata oluştu!");
                throw;
            }
        }

        // Tüm İzinleri (Permissions) Getirir
        public async Task<List<Permission>> GetAllPermissionsAsync()
        {
            _logger.LogInformation("Tüm sistem izinleri (Permissions) listeleniyor.");

            try
            {
                var permissions = await _context.Permissions
                    .AsNoTracking()
                    .Where(p => p.IsActive)
                    .ToListAsync();

                _logger.LogInformation("Sistem izinleri başarıyla getirildi. Toplam İzin Sayısı: {PermissionCount}", permissions.Count);
                return permissions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sistem izinleri çekilirken bir hata oluştu!");
                throw;
            }
        }

        // Yeni Rol Oluşturur ve İzinlerini Bağlar
        public async Task<(bool IsSuccess, string? ErrorMessage, object? ResponseData)> CreateRoleAsync(CreateRoleDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
            {
                _logger.LogWarning("Geçersiz rol oluşturma isteği gönderildi. Rol adı boş olamaz.");
                return (false, "Rol adı boş olamaz.", null);
            }

            _logger.LogInformation("Yeni rol oluşturma işlemi başlatıldı. Rol Adı: {RoleName}, Atanacak İzin Sayısı: {PermissionCount}",
                dto.Name, dto.PermissionIds?.Count ?? 0);

            try
            {
                var role = new Role
                {
                    Name = dto.Name,
                    Description = dto.Description,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.Roles.AddAsync(role);
                await _context.SaveChangesAsync();

                if (dto.PermissionIds != null && dto.PermissionIds.Any())
                {
                    var rolePermissions = dto.PermissionIds.Select(permId => new RolePermission
                    {
                        RoleId = role.Id,
                        PermissionId = permId
                    });

                    await _context.RolePermissions.AddRangeAsync(rolePermissions);
                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation("Yeni rol başarıyla oluşturuldu ve izinler bağlandı. Rol Adı: {RoleName}", dto.Name);

                // Orijinal Controller response çıktısı
                var response = new { message = "Rol başarıyla oluşturuldu." };
                return (true, null, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Yeni rol oluşturulurken bir hata oluştu! Rol Adı: {RoleName}", dto.Name);
                throw;
            }
        }
    }
}