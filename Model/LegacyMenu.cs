using System.Text.Json.Serialization;

namespace SpeedLR.Model
{
    public class LegacyMenu
    {
        public LegacyMenu(string name) { 
            Name = name;
            Id = Guid.NewGuid().ToString();
        }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("buttons")]
        public List<BaseButton> Buttons { get; set; } = new List<BaseButton>();
    }
}
