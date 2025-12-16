using System.Text.Json.Serialization;

namespace SpeedLR.Model
{
	public class PluginEnvironment
	{
		[JsonPropertyName("isDebug")]
		public Boolean IsDebug { get; set; } = false;

		[JsonPropertyName("app")]
		public String App { get; set; } = "Adobe Photoshop Lightroom Classic";
	}
}
