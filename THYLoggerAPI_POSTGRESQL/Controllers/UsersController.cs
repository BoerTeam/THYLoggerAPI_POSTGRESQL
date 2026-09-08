using Microsoft.AspNetCore.Mvc;
using THYLoggerAPI_POSTGRESQL.DTOs;
using THYLoggerAPI_POSTGRESQL.Services;

namespace THYLoggerAPI_POSTGRESQL.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(UserService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        // GET: api/Users
        // Tüm otomatik kaydolmuş kullanıcıları getirir
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            _logger.LogInformation("Tüm veritabanı kullanıcıları listeleniyor.");

            try
            {
                var users = await _userService.GetAllUsersAsync();
                _logger.LogInformation("Kullanıcılar başarıyla getirildi. Toplam Kullanıcı Sayısı: {UserCount}", users.Count);

                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı listesi çekilirken bir hata oluştu!");
                return StatusCode(500, "Kullanıcılar alınırken bir sunucu hatası oluştu: " + ex.Message);
            }
        }

        // POST: api/Users/assign-roles
        // Seçilen kullanıcıya rol ataması yapar
        [HttpPost("assign-roles")]
        public async Task<IActionResult> AssignRoles([FromBody] AssignRoleDto dto)
        {
            if (dto == null || dto.UserId <= 0)
            {
                _logger.LogWarning("Geçersiz rol atama isteği gönderildi.");
                return BadRequest("Geçersiz kullanıcı bilgisi.");
            }

            _logger.LogInformation("Kullanıcıya rol atama işlemi başlatıldı. UserId: {TargetUserId}, Atanacak Rol Sayısı: {RoleCount}",
                dto.UserId, dto.RoleIds?.Count ?? 0);

            try
            {
                var result = await _userService.AssignRolesToUserAsync(dto);

                if (!result)
                {
                    _logger.LogWarning("Rol atama başarısız. Kullanıcı bulunamadı! UserId: {TargetUserId}", dto.UserId);
                    return NotFound(new { message = "Kullanıcı bulunamadı." });
                }

                _logger.LogInformation("Roller başarıyla güncellendi. UserId: {TargetUserId}", dto.UserId);
                return Ok(new { message = "Roller başarıyla güncellendi." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Rol atama işlemi sırasında bir hata oluştu! UserId: {TargetUserId}", dto.UserId);
                return StatusCode(500, "Rol atanırken bir sunucu hatası oluştu: " + ex.Message);
            }
        }
    }
}