using SpeedLR.Model;
using System.Windows;

namespace SpeedLR.Controls
{
    public class MenuControlButton : ControlButton
    {
        public MenuControlButton(Menu menu): base(ExtractTooltipContent(menu))
        {
            Content = new string(menu.Name.Split(' ')
                                      .Where(word => !string.IsNullOrEmpty(word))
                                      .Select(word => char.ToUpper(word[0]))
                                      .ToArray()); ;
            MenuCommand = menu.Id;
        }

        private static string ExtractTooltipContent(Menu menu)
        {
            return $"Open {menu.Name}";
        }

        public static readonly DependencyProperty MenuCommandProperty =
            DependencyProperty.Register(
                nameof(MenuCommand),
                typeof(string),
                typeof(MenuControlButton),
                new PropertyMetadata(""));

        public string MenuCommand
        {
            get => (string)GetValue(MenuCommandProperty);
            set => SetValue(MenuCommandProperty, value);
        }
    }
}
