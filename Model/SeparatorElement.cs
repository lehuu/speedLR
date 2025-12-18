using System.Text.Json.Serialization;

namespace SpeedLR.Model
{
    public class SeparatorElement : MenuElement
    {
		[JsonPropertyName("isSeparator")]
		public bool IsSeparator { get; set; } = true;
	}
}