using Microsoft.EntityFrameworkCore;
using THYLoggerAPI_POSTGRESQL.Context;
using THYLoggerAPI_POSTGRESQL.DTOs;
using THYLoggerAPI_POSTGRESQL.Model;

namespace THYLoggerAPI_POSTGRESQL.Services
{
    public class AuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuthService> _logger;

        public AuthService(ApplicationDbContext context, ILogger<AuthService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<UserLoginResponseDto> LoginAsync(LoginRequestDto request)
        {
            // 1. Parametre Doğrulaması
            if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                _logger.LogWarning("Eksik veya geçersiz login parametreleri ile istek atıldı.");
                return new UserLoginResponseDto
                {
                    IsSuccess = false,
                    Message = "Kullanıcı adı veya şifre boş olamaz."
                };
            }

            _logger.LogInformation("Kullanıcı giriş denemesi başlatıldı. Kullanıcı Adı: {Username}", request.Username);

            try
            {
                // 2. LDAP Şifre Doğrulaması
                bool isLdapValid = ValidateLdapUser(request.Username, request.Password);

                if (!isLdapValid)
                {
                    _logger.LogWarning("Başarısız kullanıcı giriş denemesi (LDAP Hatası). Kullanıcı Adı: {Username}", request.Username);
                    return new UserLoginResponseDto
                    {
                        IsSuccess = false,
                        Message = "Kullanıcı adı veya şifre hatalı."
                    };
                }

                // 3. Kullanıcı Veritabanında Var mı Kontrol Et
                var user = await _context.Users
                    .Include(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                            .ThenInclude(r => r.RolePermissions)
                                .ThenInclude(rp => rp.Permission)
                    .FirstOrDefaultAsync(u => u.UserName.ToLower() == request.Username.ToLower());

                // 4. İlk Kez Giriş Yapıyorsa Veritabanına Otomatik Kaydet (Yöntem A)
                if (user == null)
                {
                    _logger.LogInformation("Kullanıcı veritabanında bulunamadı, otomatik kaydetme başlatılıyor. Kullanıcı Adı: {Username}", request.Username);

                    user = new User
                    {
                        UserName = request.Username,
                        Email = $"{request.Username}@company.com",
                        FirstName = request.Username,
                        LastName = "",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Yeni kullanıcı başarıyla veritabanına kaydedildi. UserID: {UserId}, Kullanıcı Adı: {Username}", user.Id, user.UserName);
                }

                // 5. Pasif Hesap Kontrolü
                if (!user.IsActive)
                {
                    _logger.LogWarning("Pasif durumdaki kullanıcı giriş yapmaya çalıştı. UserID: {UserId}, Kullanıcı Adı: {Username}", user.Id, user.UserName);
                    return new UserLoginResponseDto
                    {
                        IsSuccess = false,
                        Message = "Kullanıcı hesabınız pasife alınmıştır."
                    };
                }

                // 6. Kullanıcının Rol ve İzin Kodlarını Topla
                var roles = user.UserRoles?.Select(ur => ur.Role.Name).ToList() ?? new List<string>();
                var permissions = user.UserRoles?
                    .SelectMany(ur => ur.Role.RolePermissions)
                    .Select(rp => rp.Permission.Code)
                    .Distinct()
                    .ToList() ?? new List<string>();

                _logger.LogInformation("Kullanıcı başarıyla giriş yaptı. UserID: {UserId}, Kullanıcı Adı: {Username}, Rol Sayısı: {RoleCount}, İzin Sayısı: {PermissionCount}",
                    user.Id, user.UserName, roles.Count, permissions.Count);

                return new UserLoginResponseDto
                {
                    IsSuccess = true,
                    Message = "Giriş başarılı.",
                    UserId = user.Id,
                    UserName = user.UserName,
                    Roles = roles,
                    Permissions = permissions
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login işlemi sırasında beklenmeyen bir veritabanı/sunucu hatası oluştu. Kullanıcı Adı: {Username}", request.Username);
                throw; // Exception'ı Controller'a ileterek HTTP 500 yönetimine olanak sağlıyoruz.
            }
        }

        private bool ValidateLdapUser(string username, string password)
        {
            // Test ortamında admin / Admin123! veya herhangi bir kullanıcı adı kabul edilsin:
            if (username == "admin" && password == "Admin123!") return true;

            // Gerçek LDAP bağlandığında buraya Novell.Directory.Ldap kodu gelecek
            return !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password);
        }
    }
}