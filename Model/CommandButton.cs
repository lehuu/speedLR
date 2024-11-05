using System.Text.Json.Serialization;

namespace SpeedLR.Model
{
    public class CommandButton
    {
        public CommandButton(Command command, int menuIndex, int buttonIndex, string backgroundColor, string fontColor)
        {
            Command = command;
            MenuIndex = menuIndex;
            ButtonIndex = buttonIndex;
            BackgroundColor = backgroundColor;
            FontColor = fontColor;
        }

        [JsonPropertyName("command")]
        public Command Command { get; set; }

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
