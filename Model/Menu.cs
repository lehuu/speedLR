using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace SpeedLR.Model
{
	public class Menu: AbstractMenu
	{
		protected ObservableCollection<Submenu> _submenus;
		public Menu(string name, int position): base(name, position) { 
			_submenus = new ObservableCollection<Submenu>();
		}

		[JsonPropertyName("submenus")]
		public ObservableCollection<Submenu> Submenus
		{
			get => _submenus;
			set
			{
				if (_submenus != value)
				{
					_submenus = value;
					OnPropertyChanged();
				}
			}
		}
	}
}
