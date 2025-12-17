using System.Text.Json.Serialization;

namespace SpeedLR.Model
{
    public class AvailableMenus
    {
        private List<LegacyMenu> _menus = new List<LegacyMenu>();

        [JsonPropertyName("defaultMenu")]
        public string DefaultMenu { get; set; }

        [JsonPropertyName("menus")]
        public List<LegacyMenu> Menus
        {
            get
            {
                if (_menus == null || _menus.Count == 0)
                {
                    var defaultMenu = new Model.LegacyMenu("Start");
                    _menus = new List<LegacyMenu>() { defaultMenu };
                    DefaultMenu = defaultMenu.Id;
                }

                return _menus;
            }
            set
            {
                _menus = value;
            }
        }

        public void UpdateMenu(LegacyMenu menu)
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
