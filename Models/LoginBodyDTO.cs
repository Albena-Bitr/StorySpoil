using System.Text.Json.Serialization;

namespace StorySpoilAPITests.Models
{
    public class LoginBodyDto
    {
        [JsonPropertyName("userName")]
        public required string User { get; set; }

        [JsonPropertyName("password")]
        public required string Password { get; set; }
    }
}
