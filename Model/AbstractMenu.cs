using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace SpeedLR.Model
{
	public abstract class AbstractMenu: INotifyPropertyChanged, IEquatable<AbstractMenu>
	{
		protected string _name;
		protected int _position;

		public AbstractMenu(string name, int position)
		{
			_name = name;
			_position = position;
		}

		[JsonIgnore]
		public string Id { get; } = Guid.NewGuid().ToString();

		[JsonPropertyName("name")]
		public string Name
		{
			get => _name;
			set
			{
				if (_name != value)
				{
					_name = value;
					OnPropertyChanged();
				}
			}
		}

		[JsonPropertyName("position")]
		public int Position
		{
			get => _position;
			set
			{
				if (_position != value)
				{
					_position = value;
					OnPropertyChanged();
				}
			}
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		public bool Equals(AbstractMenu? other)
		{
			if (other is null) return false;
			if (ReferenceEquals(this, other)) return true;

			return Id == other.Id;
		}

		public override bool Equals(object? obj)
		{
			return Equals(obj as AbstractMenu);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Id);
		}

		protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

	}
}
