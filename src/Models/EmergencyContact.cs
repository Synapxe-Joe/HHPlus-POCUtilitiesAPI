using System.Text.Json.Serialization;

namespace POC.Utilities.API.Models
{
    public class EmergencyContact
    {
        [JsonPropertyName("name")]
        public required string Name { get; set; }
        [JsonPropertyName("contact")]
        public required string Contact { get; set; }
        [JsonPropertyName("mode")]
        public string? Mode { get; set; } = "wallet";
    }
}
