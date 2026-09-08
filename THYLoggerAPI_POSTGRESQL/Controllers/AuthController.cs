using Microsoft.AspNetCore.Mvc;
using THYLoggerAPI_POSTGRESQL.DTOs;
using THYLoggerAPI_POSTGRESQL.Services;

namespace THYLoggerAPI_POSTGRESQL.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        // POST: api/Auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Username))
            {
                _logger.LogWarning("Eksik veya geçersiz login parametreleri ile istek atıldı.");
                return BadRequest(new { Message = "Kullanıcı adı veya şifre boş olamaz." });
            }

            _logger.LogInformation("Kullanıcı giriş denemesi başlatıldı. Kullanıcı Adı: {Username}", request.Username);

            try
            {
                var result = await _authService.LoginAsync(request);

                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Başarısız kullanıcı giriş denemesi. Kullanıcı Adı: {Username}, Sebep: {Message}",
                        request.Username, result.Message);

                    return BadRequest(result);
                }

                _logger.LogInformation("Kullanıcı başarıyla giriş yaptı. Kullanıcı Adı: {Username}", request.Username);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login işlemi sırasında beklenmeyen bir hata oluştu. Kullanıcı Adı: {Username}", request.Username);
                return StatusCode(500, new { Message = "Giriş işlemi sırasında sunucu hatası oluştu: " + ex.Message });
            }
        }
    }
}