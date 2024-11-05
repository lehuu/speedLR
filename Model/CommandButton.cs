using System.Text.Json.Serialization;

namespace SpeedLR.Model
{
    public class CommandButton
    {
        public CommandButton(Command command, int menuIndex, int buttonIndex)
        {
            Command = command;
            MenuIndex = menuIndex;
            ButtonIndex = buttonIndex;
        }

        [JsonPropertyName("command")]
        public Command Command { get; set; }

        [JsonPropertyName("menuIndex")]
        public int MenuIndex { get; set; }

        [JsonPropertyName("buttonIndex")]
        public int ButtonIndex { get; set; }
    }
}
