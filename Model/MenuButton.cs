using System.Text.Json.Serialization;

namespace SpeedLR.Model
{
    public class MenuButton: BaseButton
    {
        public MenuButton(string submenu, int menuIndex, int buttonIndex, string backgroundColor, string fontColor) : base(menuIndex, buttonIndex, backgroundColor, fontColor)
        {
            Submenu = submenu;
        }

        [JsonPropertyName("submenu")]
        public string Submenu { get; set; }
    }
}
