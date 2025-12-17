using System.Text.Json.Serialization;
using SpeedLR.Utils;

namespace SpeedLR.Model
{
	[JsonConverter(typeof(JsonUserMenusConverter))]
	public class UserMenus
	{
		public UserMenus()
		{
			Menus = new List<Menu>();
		}


		[JsonIgnore] // Hide from JSON since converter handles it
		public List<Menu> Menus
		{
			get; set;
		}
	}
}
