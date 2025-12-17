using SpeedLR.Model;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SpeedLR.Utils
{
    public class JsonButtonConverter: JsonConverter<BaseButton>
    {
        public override BaseButton Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
            {
                var root = doc.RootElement;
                var typeProperty = root.GetProperty("Type").GetString();

                // Deserialize based on the "Type" property
                return typeProperty switch
                {
                    nameof(CommandButton) => JsonSerializer.Deserialize<CommandButton>(root.GetRawText(), options),
                    nameof(MenuButton) => JsonSerializer.Deserialize<MenuButton>(root.GetRawText(), options),
                    _ => throw new NotSupportedException($"Unknown type: {typeProperty}")
                };
            }
        }

        public override void Write(Utf8JsonWriter writer, BaseButton value, JsonSerializerOptions options)
        {
            var type = value.GetType();
            var json = JsonSerializer.Serialize(value, type, options);

            using (JsonDocument doc = JsonDocument.Parse(json))
            {
                writer.WriteStartObject();
                writer.WriteString("Type", type.Name); // Add type information
                foreach (var prop in doc.RootElement.EnumerateObject())
                {
                    prop.WriteTo(writer);
                }
                writer.WriteEndObject();
            }
        }
    }
}
