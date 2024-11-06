using System.Text.Json.Serialization;

namespace SpeedLR.Model
{
    public class AvailableMenus
    {
        private List<Menu> _menus = new List<Menu>();

        [JsonPropertyName("menus")]
        public List<Menu> Menus
        {
            get
            {
                if (_menus == null || _menus.Count == 0)
                {
                    _menus = new List<Menu>() { new Model.Menu("Start") };
                }

                return _menus;
            }
            set
            {
                _menus = value;
            }
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
