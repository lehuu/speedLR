using System.Text.Json.Serialization;

namespace SpeedLR.Model
{
    public class AvailableMenus
    {
        [JsonPropertyName("menus")]
        public List<Menu> Menus { get; set; } = new List<Menu>();

        public void UpdateMenu(Menu menu)
        {
            var existingMenuIndex = Menus.FindIndex(item => item.Name == menu.Name);

            if(existingMenuIndex != -1)
            {
                Menus[existingMenuIndex] = menu;
            } else
            {
                Menus.Add(menu);
            }
        }   
    }
}
