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

        // Tüm Rolleri ve İzinlerini DTO olarak güvenli getirir
        public async Task<List<RoleDto>> GetAllRolesAsync()
        {
            _logger.LogInformation("Tüm sistem rolleri listeleniyor.");

            try
            {
                var roles = await _context.Roles
                    .AsNoTracking()
                    .Include(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
                    .Select(r => new RoleDto
                    {
                        Id = r.Id,
                        Name = r.Name,
                        Description = r.Description,
                        IsActive = r.IsActive,
                        RolePermissions = r.RolePermissions.Select(rp => new RolePermissionDto
                        {
                            RoleId = rp.RoleId,
                            PermissionId = rp.PermissionId,
                            Permission = rp.Permission != null ? new PermissionDto
                            {
                                Id = rp.Permission.Id,
                                Name = rp.Permission.Name,
                                Code = rp.Permission.Code
                            } : null
                        }).ToList()
                    })
                    .ToListAsync();

                return roles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sistem rolleri çekilirken bir hata oluştu!");
                throw;
            }
        }

        // Tüm İzinleri Getirir
        public async Task<List<PermissionDto>> GetAllPermissionsAsync()
        {
            try
            {
                return await _context.Permissions
                    .AsNoTracking()
                    .Where(p => p.IsActive)
                    .Select(p => new PermissionDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Code = p.Code
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sistem izinleri çekilirken bir hata oluştu!");
                throw;
            }
        }

        // Yeni Rol Oluşturur
        public async Task<(bool IsSuccess, string? ErrorMessage, object? ResponseData)> CreateRoleAsync(CreateRoleDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
            {
                return (false, "Rol adı boş olamaz.", null);
            }

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

                return (true, null, new { message = "Rol başarıyla oluşturuldu." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Yeni rol oluşturulurken hata oluştu! Rol Adı: {RoleName}", dto.Name);
                throw;
            }
        }
        // Rol Güncelleme Metodu
        public async Task<(bool IsSuccess, string? ErrorMessage)> UpdateRoleAsync(UpdateRoleDto dto)
        {
            if (dto == null || dto.Id <= 0 || string.IsNullOrWhiteSpace(dto.Name))
            {
                return (false, "Geçersiz rol verisi.");
            }

            try
            {
                var role = await _context.Roles
                    .Include(r => r.RolePermissions)
                    .FirstOrDefaultAsync(r => r.Id == dto.Id);

                if (role == null)
                {
                    return (false, "Güncellenecek rol bulunamadı.");
                }

                // Rol temel bilgilerini güncelle
                role.Name = dto.Name;
                role.Description = dto.Description;
                role.UpdatedAt = DateTime.UtcNow;

                // Eski izinleri temizle
                _context.RolePermissions.RemoveRange(role.RolePermissions);

                // Yeni seçilen izinleri ekle
                if (dto.PermissionIds != null && dto.PermissionIds.Any())
                {
                    var newPermissions = dto.PermissionIds.Select(pId => new RolePermission
                    {
                        RoleId = role.Id,
                        PermissionId = pId
                    });
                    await _context.RolePermissions.AddRangeAsync(newPermissions);
                }

                await _context.SaveChangesAsync();
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Rol güncellenirken hata oluştu! Rol ID: {RoleId}", dto.Id);
                throw;
            }
        }

        // Rol Silme Metodu
        public async Task<(bool IsSuccess, string? ErrorMessage)> DeleteRoleAsync(int id)
        {
            try
            {
                var role = await _context.Roles.FindAsync(id);
                if (role == null)
                {
                    return (false, "Silinecek rol bulunamadı.");
                }

                // Sistem Rolü ise silinmesini engelle (Admin gibi)
                if (role.IsSystemRole)
                {
                    return (false, "Sistem rolleri silinemez.");
                }

                _context.Roles.Remove(role);
                await _context.SaveChangesAsync();
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Rol silinirken hata oluştu! Rol ID: {RoleId}", id);
                throw;
            }
        }
    }
}