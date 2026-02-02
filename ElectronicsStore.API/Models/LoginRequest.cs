using System.Text.Json.Serialization;

namespace ElectronicsStore.API.Models
{
    public class LoginRequest
    {
        public string? Email { get; set; }

        // Primary property used throughout the API (Vietnamese naming)
        [JsonPropertyName("MatKhau")]
        public string? MatKhau { get; set; }

        // Accept "password" from clients that send English field name (setter only)
        [JsonPropertyName("password")]
        public string? Password { set => MatKhau = value; }
    }
}