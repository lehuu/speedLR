using System.Text.Json.Serialization;

namespace SpeedLR.Model
{
    public class CommandButton: BaseButton
    {
        public CommandButton(Command command, int menuIndex, int buttonIndex, string backgroundColor, string fontColor): base(menuIndex, buttonIndex, backgroundColor, fontColor)
        {
            Command = command;
        }

        [JsonPropertyName("command")]
        public Command Command { get; set; }
    }
}
