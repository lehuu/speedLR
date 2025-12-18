using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace SpeedLR.Model
{
	[JsonDerivedType(typeof(ActionElement), typeDiscriminator: "action")]
	[JsonDerivedType(typeof(SeparatorElement), typeDiscriminator: "separator")]
	public abstract class MenuElement : IEquatable<MenuElement>
	{
		[JsonIgnore]
		public string Id { get; } = Guid.NewGuid().ToString();

		[JsonPropertyName("Position")]
		public int Position { get; set; }

		public event PropertyChangedEventHandler? PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

		public bool Equals(MenuElement? other)
		{
			if (other is null) return false;
			if (ReferenceEquals(this, other)) return true;

			return Id == other.Id;
		}

		public override bool Equals(object? obj)
		{
			return Equals(obj as MenuElement);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Id);
		}
	}
}
