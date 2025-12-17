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

		public void UpdateMenu(Menu menu)
		{
			var existingMenuIndex = Menus.FindIndex(item => item.Id == menu.Id);

			if (existingMenuIndex != -1)
			{
				Menus[existingMenuIndex] = menu;
			}
			else
			{
				Menus.Add(menu);
			}
		}
	}
}
