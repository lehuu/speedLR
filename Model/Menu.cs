using System.Text.Json.Serialization;

namespace SpeedLR.Model
{
	public class Menu: AbstractMenu
	{
		protected List<Submenu> _submenus;
		public Menu(string name, int position): base(name, position) { 
			_submenus = new List<Submenu>();
		}

		[JsonPropertyName("submenus")]
		public List<Submenu> Submenus
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
