using System.Text.Json.Serialization;

namespace SpeedLR.Model
{
    public class Category
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("commands")]
        public List<Command> Commands { get; set; } = new List<Command>();
	}
}
