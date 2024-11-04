using System.Text.Json.Serialization;

namespace SpeedLR.Model
{
    public class AvailableCommands
    {
        [JsonPropertyName("categories")]
        public List<Category> Categories { get; set; } = new List<Category>();
    }
}
