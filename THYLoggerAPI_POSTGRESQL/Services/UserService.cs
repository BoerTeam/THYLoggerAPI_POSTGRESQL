using Microsoft.EntityFrameworkCore;
using THYLoggerAPI_POSTGRESQL.Context;
using THYLoggerAPI_POSTGRESQL.DTOs;
using THYLoggerAPI_POSTGRESQL.Model;

namespace THYLoggerAPI_POSTGRESQL.Services
{
    public class UserService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserService> _logger;

        public UserService(ApplicationDbContext context, ILogger<UserService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Otomatik kaydolmuş tüm kullanıcıları listeler
        public async Task<List<UserListDto>> GetAllUsersAsync()
        {
            _logger.LogInformation("Tüm veritabanı kullanıcıları listeleniyor.");

            try
            {
                var users = await _context.Users
                    .AsNoTracking()
                    .Select(u => new UserListDto
                    {
                        Id = u.Id,
                        UserName = u.UserName,
                        Email = u.Email,
                        IsActive = u.IsActive,
                        Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList()
                    })
                    .ToListAsync();

                _logger.LogInformation("Kullanıcılar başarıyla getirildi. Toplam Kullanıcı Sayısı: {UserCount}", users.Count);
                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı listesi çekilirken bir hata oluştu!");
                throw;
            }
        }

        // Seçilen kullanıcıya roller atar
        public async Task<(bool IsSuccess, bool IsNotFound, string? ErrorMessage, object? ResponseData)> AssignRolesToUserAsync(AssignRoleDto dto)
        {
            if (dto == null || dto.UserId <= 0)
            {
                _logger.LogWarning("Geçersiz rol atama isteği gönderildi.");
                return (false, false, "Geçersiz kullanıcı bilgisi.", null);
            }

            _logger.LogInformation("Kullanıcıya rol atama işlemi başlatıldı. UserId: {TargetUserId}, Atanacak Rol Sayısı: {RoleCount}",
                dto.UserId, dto.RoleIds?.Count ?? 0);

            try
            {
                var user = await _context.Users
                    .Include(u => u.UserRoles)
                    .FirstOrDefaultAsync(u => u.Id == dto.UserId);

                if (user == null)
                {
                    _logger.LogWarning("Rol atama başarısız. Kullanıcı bulunamadı! UserId: {TargetUserId}", dto.UserId);
                    return (false, true, "Kullanıcı bulunamadı.", null);
                }

                // Mevcut rolleri temizle
                _context.UserRoles.RemoveRange(user.UserRoles);

                // Yeni seçilen rolleri ekle
                if (dto.RoleIds != null && dto.RoleIds.Any())
                {
                    var newRoles = dto.RoleIds.Select(roleId => new UserRole
                    {
                        UserId = dto.UserId,
                        RoleId = roleId
                    });

                    await _context.UserRoles.AddRangeAsync(newRoles);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Roller başarıyla güncellendi. UserId: {TargetUserId}", dto.UserId);

                var response = new { message = "Roller başarıyla güncellendi." };
                return (true, false, null, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Rol atama işlemi sırasında bir hata oluştu! UserId: {TargetUserId}", dto.UserId);
                throw;
            }
        }

        // Kullanıcının mevcut ve tüm rollerini getiren metod (Dashboard ekranı için)
        public async Task<(bool IsSuccess, bool IsNotFound, string? ErrorMessage, UserRoleDetailDto? Data)> GetUserRolesForAssignAsync(int userId)
        {
            if (userId <= 0)
            {
                _logger.LogWarning("Geçersiz kullanıcı ID'si gönderildi. UserId: {UserId}", userId);
                return (false, false, "Geçersiz kullanıcı bilgisi.", null);
            }

            _logger.LogInformation("Kullanıcı rol detayları getiriliyor. UserId: {UserId}", userId);

            try
            {
                var user = await _context.Users
                    .AsNoTracking()
                    .Include(u => u.UserRoles)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    _logger.LogWarning("Kullanıcı bulunamadı! UserId: {UserId}", userId);
                    return (false, true, "Kullanıcı bulunamadı.", null);
                }

                var allRoles = await _context.Roles
                    .AsNoTracking()
                    .Where(r => r.IsActive)
                    .ToListAsync();

                var userRoleIds = user.UserRoles.Select(ur => ur.RoleId).ToList();

                var result = new UserRoleDetailDto
                {
                    UserId = user.Id,
                    Username = user.UserName,
                    Roles = allRoles.Select(r => new RoleItemDto
                    {
                        RoleId = r.Id,
                        RoleName = r.Name,
                        IsAssigned = userRoleIds.Contains(r.Id)
                    }).ToList()
                };

                _logger.LogInformation("Kullanıcı rol detayları başarıyla getirildi. UserId: {UserId}", userId);
                return (true, false, null, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı rol detayları çekilirken bir hata oluştu! UserId: {UserId}", userId);
                throw;
            }
        }

        // SSO İle Giriş Yapan Kullanıcıyı Bulur veya Otomatik Oluşturur (JIT Provisioning)
        public async Task<SsoUserResponseDto> GetOrCreateSsoUserAsync(SsoUserDto dto)
        {
            _logger.LogInformation("SSO kullanıcısı sorgulanıyor/oluşturuluyor. Username: {Username}", dto.Username);

            var user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                        .ThenInclude(r => r.RolePermissions)
                            .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(u => u.UserName == dto.Username);

            // Kullanıcı veritabanında yoksa otomatik kaydediyoruz
            if (user == null)
            {
                _logger.LogInformation("SSO kullanıcısı veritabanında bulunamadı, yeni kayıt oluşturuluyor. Username: {Username}", dto.Username);

                user = new User
                {
                    UserName = dto.Username,
                    Email = dto.Email,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Varsayılan rol ataması yapıyoruz (örneğin "User" rolü varsa)
                var defaultRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
                if (defaultRole != null)
                {
                    _context.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = defaultRole.Id });
                    await _context.SaveChangesAsync();

                    // Yeniden ilişkileri yüklemek için include sorgusunu tazeliyoruz
                    user = await _context.Users
                        .Include(u => u.UserRoles)
                            .ThenInclude(ur => ur.Role)
                                .ThenInclude(r => r.RolePermissions)
                                    .ThenInclude(rp => rp.Permission)
                        .FirstOrDefaultAsync(u => u.Id == user.Id);
                }
            }

            var roles = user?.UserRoles?.Select(ur => ur.Role.Name).ToList() ?? new List<string>();
            var permissions = user?.UserRoles?
                .SelectMany(ur => ur.Role.RolePermissions)
                .Select(rp => rp.Permission.Code)
                .Distinct()
                .ToList() ?? new List<string>();

            return new SsoUserResponseDto
            {
                UserId = user!.Id,
                UserName = user.UserName,
                Roles = roles,
                Permissions = permissions
            };
        }
    }
}