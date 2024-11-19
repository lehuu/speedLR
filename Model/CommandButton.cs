using System.Text.Json.Serialization;

namespace SpeedLR.Model
{
    public class CommandButton: BaseButton
    {
        public CommandButton(Command command, int row, int col, string backgroundColor, string fontColor): base(row, col, backgroundColor, fontColor)
        {
            Command = command;
        }

        [JsonPropertyName("command")]
        public Command Command { get; set; }
    }
}
