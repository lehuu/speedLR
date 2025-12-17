using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace SpeedLR.Model
{
	public class Menu: INotifyPropertyChanged
	{
		private string _name;
		private int _position;

		public Menu(string name, int position)
		{
			Name = name;
			Id = Guid.NewGuid().ToString();
			Position = position;
		}

		[JsonPropertyName("id")]
		public string Id { get; set; }

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

		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

	}
}
