using Dashboard.Models;
using Microsoft.Extensions.Options;
using System.DirectoryServices.Protocols;
using System.Net;

namespace Dashboard.Services
{
    public interface ILdapAuthenticationService
    {
        Task<LdapAuthenticationResult> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default);
    }

    public sealed class LdapAuthenticationResult
    {
        public bool Succeeded { get; init; }
        public string Username { get; init; } = string.Empty;
        public string? DisplayName { get; init; }
        public string? Email { get; init; }
        public string? ErrorMessage { get; init; }
    }

    public class LdapAuthenticationService : ILdapAuthenticationService
    {
        private readonly LdapAuthenticationOptions _options;
        private readonly ILogger<LdapAuthenticationService> _logger;

        public LdapAuthenticationService(
            IOptions<LdapAuthenticationOptions> options,
            ILogger<LdapAuthenticationService> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public Task<LdapAuthenticationResult> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default)
        {
            var trimmedUsername = username.Trim();
            if (string.IsNullOrWhiteSpace(_options.Host) || _options.Host.StartsWith("<set-via-env:", StringComparison.Ordinal))
            {
                return Task.FromResult(new LdapAuthenticationResult
                {
                    ErrorMessage = "LDAP bağlantı ayarları tamamlanmadığı için giriş yapılamıyor."
                });
            }

            var bindUsername = BuildBindUsername(trimmedUsername);

            try
            {
                using var connection = new LdapConnection(new LdapDirectoryIdentifier(_options.Host, _options.Port))
                {
                    AuthType = AuthType.Basic,
                    Credential = new NetworkCredential(bindUsername, password)
                };

                connection.SessionOptions.ProtocolVersion = 3;
                connection.SessionOptions.SecureSocketLayer = _options.UseSsl;
                connection.Bind();

                var result = new LdapAuthenticationResult
                {
                    Succeeded = true,
                    Username = trimmedUsername
                };

                if (!string.IsNullOrWhiteSpace(_options.BaseDn))
                {
                    result = PopulateDirectoryDetails(connection, trimmedUsername, result);
                }

                return Task.FromResult(result);
            }
            catch (LdapException ex) when (ex.ErrorCode == 49)
            {
                _logger.LogWarning(ex, "LDAP login failed for user {Username}", trimmedUsername);
                return Task.FromResult(new LdapAuthenticationResult
                {
                    ErrorMessage = "Kullanıcı adı veya şifre hatalı."
                });
            }
            catch (LdapException ex)
            {
                _logger.LogError(ex, "LDAP connection error for user {Username}", trimmedUsername);
                return Task.FromResult(new LdapAuthenticationResult
                {
                    ErrorMessage = "LDAP servisine bağlanırken bir hata oluştu. Lütfen daha sonra tekrar deneyin."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected LDAP login error for user {Username}", trimmedUsername);
                return Task.FromResult(new LdapAuthenticationResult
                {
                    ErrorMessage = "Giriş işlemi sırasında beklenmeyen bir hata oluştu."
                });
            }
        }

        private string BuildBindUsername(string username)
        {
            if (username.Contains('@') || string.IsNullOrWhiteSpace(_options.Domain) || _options.Domain.StartsWith("<set-via-env:", StringComparison.Ordinal))
            {
                return username;
            }

            return $"{username}@{_options.Domain}";
        }

        private LdapAuthenticationResult PopulateDirectoryDetails(
            LdapConnection connection,
            string username,
            LdapAuthenticationResult result)
        {
            var escapedUsername = EscapeFilterValue(username.Contains('@') ? username.Split('@')[0] : username);
            var filter = string.Format(_options.SearchFilter, escapedUsername);
            var request = new SearchRequest(_options.BaseDn, filter, SearchScope.Subtree, "displayName", "mail");
            var response = (SearchResponse)connection.SendRequest(request);
            var entry = response.Entries.Cast<SearchResultEntry>().FirstOrDefault();

            if (entry is null)
            {
                return result;
            }

            return new LdapAuthenticationResult
            {
                Succeeded = result.Succeeded,
                Username = result.Username,
                DisplayName = entry.Attributes["displayName"]?[0]?.ToString(),
                Email = entry.Attributes["mail"]?[0]?.ToString()
            };
        }

        private static string EscapeFilterValue(string value)
        {
            return value
                .Replace("\\", "\\5c", StringComparison.Ordinal)
                .Replace("*", "\\2a", StringComparison.Ordinal)
                .Replace("(", "\\28", StringComparison.Ordinal)
                .Replace(")", "\\29", StringComparison.Ordinal)
                .Replace("\0", "\\00", StringComparison.Ordinal);
        }
    }
}
