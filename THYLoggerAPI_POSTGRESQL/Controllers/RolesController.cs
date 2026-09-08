using Microsoft.AspNetCore.Mvc;
using THYLoggerAPI_POSTGRESQL.DTOs;
using THYLoggerAPI_POSTGRESQL.Services;

namespace THYLoggerAPI_POSTGRESQL.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly RoleService _roleService;
        private readonly ILogger<RolesController> _logger;

        public RolesController(RoleService roleService, ILogger<RolesController> logger)
        {
            _roleService = roleService;
            _logger = logger;
        }

        // GET: api/Roles
        // Tüm Rolleri Listeler
        [HttpGet]
        public async Task<IActionResult> GetAllRoles()
        {
            _logger.LogInformation("Tüm sistem rolleri listeleniyor.");

            try
            {
                var roles = await _roleService.GetAllRolesAsync();
                _logger.LogInformation("Sistem rolleri başarıyla getirildi. Toplam Rol Sayısı: {RoleCount}", roles.Count);

                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sistem rolleri çekilirken bir hata oluştu!");
                return StatusCode(500, "Roller alınırken sunucu hatası oluştu: " + ex.Message);
            }
        }

        // GET: api/Roles/permissions
        // Sistemdeki Tüm İzin Kutucuklarını (Permissions) Getirir
        [HttpGet("permissions")]
        public async Task<IActionResult> GetAllPermissions()
        {
            _logger.LogInformation("Tüm sistem izinleri (Permissions) listeleniyor.");

            try
            {
                var permissions = await _roleService.GetAllPermissionsAsync();
                _logger.LogInformation("Sistem izinleri başarıyla getirildi. Toplam İzin Sayısı: {PermissionCount}", permissions.Count);

                return Ok(permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sistem izinleri çekilirken bir hata oluştu!");
                return StatusCode(500, "İzinler alınırken sunucu hatası oluştu: " + ex.Message);
            }
        }

        // POST: api/Roles
        // Yeni Rol ve İzin Bağlantılarını Oluşturur
        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto dto)
        {
            // 'RoleName' yerine 'Name' kullanıyoruz
            if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
            {
                _logger.LogWarning("Geçersiz rol oluşturma isteği gönderildi. Rol adı boş olamaz.");
                return BadRequest("Rol adı boş olamaz.");
            }

            _logger.LogInformation("Yeni rol oluşturma işlemi başlatıldı. Rol Adı: {RoleName}, Atanacak İzin Sayısı: {PermissionCount}",
                dto.Name, dto.PermissionIds?.Count ?? 0);

            try
            {
                var result = await _roleService.CreateRoleAsync(dto);

                _logger.LogInformation("Yeni rol başarıyla oluşturuldu ve izinler bağlandı. Rol Adı: {RoleName}", dto.Name);
                return Ok(new { message = "Rol başarıyla oluşturuldu." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Yeni rol oluşturulurken bir hata oluştu! Rol Adı: {RoleName}", dto.Name);
                return StatusCode(500, "Rol oluşturulurken sunucu hatası oluştu: " + ex.Message);
            }
        }
    }
}