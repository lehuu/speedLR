using System.Text.Json.Serialization;

namespace SpeedLR.Model
{
	public class Menu
	{
		public Menu(string name)
		{
			Name = name;
			Id = Guid.NewGuid().ToString();
			Position = 0;
		}

		[JsonPropertyName("id")]
		public string Id { get; set; }

		[JsonPropertyName("name")]
		public string Name { get; set; }

		[JsonPropertyName("position")]
		public int Position { get; set; }

	}
}
