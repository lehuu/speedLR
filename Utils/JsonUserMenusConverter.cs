using System.Text.Json;
using System.Text.Json.Serialization;
using SpeedLR.Model;

namespace SpeedLR.Utils
{
	public class JsonUserMenusConverter : JsonConverter<UserMenus>
	{
		public override UserMenus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.StartArray)
				throw new JsonException("Expected root-level array");

			var menus = JsonSerializer.Deserialize<List<Menu>>(ref reader, options) ?? new List<Menu>();
			var userMenus = new UserMenus { Menus = menus }; // Use private backing field

			return userMenus;
		}

		public override void Write(Utf8JsonWriter writer, UserMenus value, JsonSerializerOptions options)
		{
			JsonSerializer.Serialize(writer, value.Menus, options);
		}
	}
}
