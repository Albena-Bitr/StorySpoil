using System.Text.Json.Serialization;

namespace StorySpoilAPITests.Models
{
    public class ApiResponseDto
    {
        [JsonPropertyName("msg")]
        public string? Msg {  get; set; }

        [JsonPropertyName("storyId")]
        public string? StoryId { get; set; }

    }
}
