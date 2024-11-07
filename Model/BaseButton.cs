using System.Text.Json.Serialization;

namespace SpeedLR.Model
{
    [JsonConverter(typeof(JsonButtonConverter))]
    public class BaseButton
    {
        public BaseButton(int menuIndex, int buttonIndex, string backgroundColor, string fontColor)
        {
            MenuIndex = menuIndex;
            ButtonIndex = buttonIndex;
            BackgroundColor = backgroundColor;
            FontColor = fontColor;
        }
        [JsonPropertyName("menuIndex")]
        public int MenuIndex { get; set; }

        [JsonPropertyName("buttonIndex")]
        public int ButtonIndex { get; set; }

        [JsonPropertyName("backgroundColor")]
        public string BackgroundColor { get; set; }

        [JsonPropertyName("fontColor")]
        public string FontColor { get; set; }
    }
}
