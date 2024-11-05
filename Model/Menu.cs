using System.Text.Json.Serialization;

namespace SpeedLR.Model
{
    public class Menu
    {
        public Menu(string name) { 
            Name = name;
        }

        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("buttons")]
        public List<CommandButton> Buttons { get; set; } = new List<CommandButton>();
    }
}
