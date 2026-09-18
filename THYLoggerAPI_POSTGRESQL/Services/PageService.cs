using Microsoft.EntityFrameworkCore;
using THYLoggerAPI_POSTGRESQL.Context;
using THYLoggerAPI_POSTGRESQL.DTOs;
using THYLoggerAPI_POSTGRESQL.Model;

namespace THYLoggerAPI_POSTGRESQL.Services
{
    public class PageService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PageService> _logger;

        public PageService(ApplicationDbContext context, ILogger<PageService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // 1. Tüm Aktif Sayfaları Sırasına Göre Listeleme
        public async Task<List<PageListDto>> GetAllPagesAsync()
        {
            _logger.LogInformation("Tüm sistem sayfaları listeleniyor.");

            try
            {
                return await _context.Pages
                    .AsNoTracking()
                    .Where(p => p.IsActive)
                    .OrderBy(p => p.Order)
                    .Select(p => new PageListDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Route = p.Route,
                        PermissionCode = p.PermissionCode,
                        Icon = p.Icon,
                        Order = p.Order,
                        IsActive = p.IsActive
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sayfalar getirilirken bir hata oluştu.");
                throw;
            }
        }

        // 2. Yeni Sayfa Tanımlama
        public async Task<(bool IsSuccess, string? ErrorMessage, object? ResponseData)> CreatePageAsync(CreatePageDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Route))
            {
                _logger.LogWarning("Geçersiz sayfa oluşturma isteği. Name ve Route boş olamaz.");
                return (false, "Sayfa adı (Name) ve Route zorunludur.", null);
            }

            try
            {
                var page = new Page
                {
                    Name = dto.Name,
                    Route = dto.Route,
                    PermissionCode = dto.PermissionCode,
                    Icon = dto.Icon,
                    Order = dto.Order,
                    IsActive = true
                };

                await _context.Pages.AddAsync(page);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Yeni sayfa eklendi. Page Name: {Name}, Route: {Route}", dto.Name, dto.Route);

                var response = new { message = "Sayfa başarıyla eklendi.", pageId = page.Id };
                return (true, null, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sayfa eklenirken sunucu hatası oluştu! Page Name: {Name}", dto.Name);
                throw;
            }
        }
        // 3. Giriş Yapan Kullanıcının Yetkili Olduğu Sayfaları Listeleme (Dinamik Menü İçin)
        public async Task<(bool IsSuccess, bool IsNotFound, string? ErrorMessage, List<PageListDto>? Data)> GetUserPagesAsync(int userId)
        {
            if (userId <= 0)
            {
                _logger.LogWarning("Geçersiz kullanıcı ID'si gönderildi. UserId: {UserId}", userId);
                return (false, false, "Geçersiz kullanıcı bilgisi.", null);
            }

            _logger.LogInformation("Kullanıcıya özel menü sayfaları çekiliyor. UserId: {UserId}", userId);

            try
            {
                var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
                if (!userExists)
                {
                    _logger.LogWarning("Kullanıcı bulunamadı. UserId: {UserId}", userId);
                    return (false, true, "Kullanıcı bulunamadı.", null);
                }

                // 1. Kullanıcının rollerinden elde edilen izin kodlarını (Permission.Code) çek
                var userPermissionCodes = await _context.UserRoles
                    .Where(ur => ur.UserId == userId)
                    .SelectMany(ur => ur.Role.RolePermissions)
                    .Select(rp => rp.Permission.Code)
                    .Distinct()
                    .ToListAsync();

                // 2. Bu izin kodlarıyla eşleşen aktif sayfaları çek ve sırala
                var userPages = await _context.Pages
                    .AsNoTracking()
                    .Where(p => p.IsActive && userPermissionCodes.Contains(p.PermissionCode))
                    .OrderBy(p => p.Order)
                    .Select(p => new PageListDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Route = p.Route,
                        PermissionCode = p.PermissionCode,
                        Icon = p.Icon,
                        Order = p.Order,
                        IsActive = p.IsActive
                    })
                    .ToListAsync();

                _logger.LogInformation("Kullanıcı sayfaları başarıyla çekildi. UserId: {UserId}, Sayfa Sayısı: {PageCount}", userId, userPages.Count);

                return (true, false, null, userPages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı sayfaları çekilirken sunucu hatası oluştu! UserId: {UserId}", userId);
                throw;
            }
        }
    }
}