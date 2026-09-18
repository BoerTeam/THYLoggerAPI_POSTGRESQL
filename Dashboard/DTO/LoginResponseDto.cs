using Newtonsoft.Json;
using System.Collections.Generic;

namespace Dashboard.DTO
{
    public class LoginResponseDto
    {
        [JsonProperty("isSuccess")]
        public bool IsSuccess { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; } = string.Empty;

        [JsonProperty("token")]
        public string? Token { get; set; }

        [JsonProperty("userId")]
        public int UserId { get; set; }

        [JsonProperty("userName")]
        public string UserName { get; set; } = string.Empty;

        [JsonProperty("roles")]
        public List<string> Roles { get; set; } = new List<string>();

        [JsonProperty("permissions")]
        public List<string> Permissions { get; set; } = new List<string>();
    }
}