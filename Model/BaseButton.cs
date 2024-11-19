using System.Text.Json.Serialization;

namespace SpeedLR.Model
{
    [JsonConverter(typeof(JsonButtonConverter))]
    public class BaseButton
    {
        public BaseButton(int row, int col, string backgroundColor, string fontColor)
        {
            Row = row;
            Col = col;
            BackgroundColor = backgroundColor;
            FontColor = fontColor;
        }
        [JsonPropertyName("row")]
        public int Row { get; set; }

        [JsonPropertyName("col")]
        public int Col { get; set; }

        [JsonPropertyName("backgroundColor")]
        public string BackgroundColor { get; set; }

        [JsonPropertyName("fontColor")]
        public string FontColor { get; set; }
    }
}
