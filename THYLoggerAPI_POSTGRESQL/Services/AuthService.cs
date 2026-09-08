using Microsoft.EntityFrameworkCore;
using THYLoggerAPI_POSTGRESQL.Context;
using THYLoggerAPI_POSTGRESQL.DTOs;
using THYLoggerAPI_POSTGRESQL.Model;

namespace THYLoggerAPI_POSTGRESQL.Services
{
    public class AuthService
    {
        private readonly ApplicationDbContext _context;

        public AuthService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserLoginResponseDto> LoginAsync(LoginRequestDto request)
        {
            // 1. LDAP Şifre Doğrulaması (Gerçek LDAP entegrasyonu buraya gelecek)
            bool isLdapValid = ValidateLdapUser(request.Username, request.Password);

            if (!isLdapValid)
            {
                return new UserLoginResponseDto { IsSuccess = false, Message = "Kullanıcı adı veya şifre hatalı." };
            }

            // 2. Kullanıcı Veritabanında Var mı Kontrol Et
            var user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                        .ThenInclude(r => r.RolePermissions)
                            .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(u => u.UserName.ToLower() == request.Username.ToLower());

            // 3. İlk Kez Giriş Yapıyorsa Veritabanına Otomatik Kaydet (Yöntem A)
            if (user == null)
            {
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
            }

            if (!user.IsActive)
            {
                return new UserLoginResponseDto { IsSuccess = false, Message = "Kullanıcı hesabınız pasife alınmıştır." };
            }

            // 4. Kullanıcının Roller ve İzin Kodlarını Topla
            var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
            var permissions = user.UserRoles
                .SelectMany(ur => ur.Role.RolePermissions)
                .Select(rp => rp.Permission.Code)
                .Distinct()
                .ToList();

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

        private bool ValidateLdapUser(string username, string password)
        {
            // Test ortamında admin / Admin123! veya herhangi bir kullanıcı adı kabul edilsin:
            if (username == "admin" && password == "Admin123!") return true;

            // Gerçek LDAP bağlandığında buraya Novell.Directory.Ldap kodu gelecek
            return !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password);
        }
    }
}