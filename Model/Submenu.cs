using System.Text.Json.Serialization;
using System.Collections.ObjectModel;

namespace SpeedLR.Model
{
	public class Submenu : AbstractMenu
	{
		private string _backgroundColor = "#D3D3D3";
		private string _fontColor = "#000000";
		private ObservableCollection<MenuElement> _items = new ObservableCollection<MenuElement>();
		public Submenu(string name, int position) : base(name, position)
		{ }

		[JsonIgnore]
		public string ShortName => string.Concat(
			Name?
				.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
				.Select(word => word[0].ToString().ToUpper()) ?? Enumerable.Empty<string>()
		);

		[JsonPropertyName("name")]
		public override string Name
		{
			get => base.Name;
			set
			{
				base.Name = value;
				OnPropertyChanged(nameof(ShortName));
			}
		}



		[JsonPropertyName("bgColor")]
		public string BackgroundColor
		{
			get
			{
				return _backgroundColor;
			}
			set
			{
				if (_backgroundColor != value)
				{
					_backgroundColor = value;
					OnPropertyChanged();
				}
			}
		}

		[JsonPropertyName("fontColor")]
		public string FontColor
		{
			get
			{
				return _fontColor;
			}
			set
			{
				if (_fontColor != value)
				{
					_fontColor = value;
					OnPropertyChanged();
				}
			}
		}

		[JsonPropertyName("items")]
		public ObservableCollection<MenuElement> Items
		{
			get
			{
				return _items;
			}
			set
			{
				if (_items != value)
				{
					_items = value;
					OnPropertyChanged();
				}
			}
		}
	}
}
