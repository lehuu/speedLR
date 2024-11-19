using System.Text.Json.Serialization;

namespace SpeedLR.Model
{
    public class MenuButton: BaseButton
    {
        public MenuButton(string submenu, int row, int col, string backgroundColor, string fontColor) : base(row, col, backgroundColor, fontColor)
        {
            Submenu = submenu;
        }

        [JsonPropertyName("submenu")]
        public string Submenu { get; set; }
    }
}
