using System.Text.Json.Serialization;

namespace StorySpoilAPITests.Models
{
    public class AuthenticationResponseDto
    {
        [JsonPropertyName("username")]
        public string? Username { get; set; }

        [JsonPropertyName("password")]
        public string? Password { get; set; }

        [JsonPropertyName("accessToken")]
        public string? AccessToken { get; set; }
    }
}
