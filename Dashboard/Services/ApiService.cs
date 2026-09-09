using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Dashboard.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ApiService> _logger;

        public ApiService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            ILogger<ApiService> logger)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        // Cookie'den Token'ı okuyup Authorization Header'ına ekleyen yardımcı metot
        private void AddAuthorizationHeader()
        {
            var token = _httpContextAccessor.HttpContext?.User.FindFirst("JWToken")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        public async Task<T?> GetAsync<T>(string endpoint, Dictionary<string, string>? queryParams = null)
        {
            try
            {
                AddAuthorizationHeader();

                var requestUrl = endpoint;
                if (queryParams != null && queryParams.Any())
                {
                    var queryString = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
                    requestUrl += $"?{queryString}";
                }

                var response = await _httpClient.GetAsync(requestUrl);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<T>(json);
                }

                _logger.LogWarning("API GET Başarısız: {Endpoint}, Status: {StatusCode}", requestUrl, response.StatusCode);
                return default;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API GET Çağrısında Hata: {Endpoint}", endpoint);
                return default;
            }
        }

        public async Task<bool> PostAsync<T>(string endpoint, T data)
        {
            try
            {
                AddAuthorizationHeader();

                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(endpoint, content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API POST Çağrısında Hata: {Endpoint}", endpoint);
                return false;
            }
        }

        // Yanıt nesnesi beklenen POST çağrıları için (Örn: Login)
        public async Task<TResult?> PostAsync<TResult, TData>(string endpoint, TData data)
        {
            try
            {
                AddAuthorizationHeader();

                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(endpoint, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<TResult>(responseJson);
                }

                _logger.LogWarning("API POST Başarısız: {Endpoint}, Status: {StatusCode}", endpoint, response.StatusCode);
                return default;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API POST Çağrısında Hata: {Endpoint}", endpoint);
                return default;
            }
        }

        public async Task<bool> PutAsync<T>(string endpoint, T data)
        {
            try
            {
                AddAuthorizationHeader();

                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync(endpoint, content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API PUT Çağrısında Hata: {Endpoint}", endpoint);
                return false;
            }
        }
    }
}