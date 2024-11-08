using SpeedLR.Model;
using System.Windows;

namespace SpeedLR.Controls
{
    public class MenuControlButton : ControlButton
    {
        public MenuControlButton(Menu menu)
        {
            Content = new string(menu.Name.Split(' ')
                                      .Where(word => !string.IsNullOrEmpty(word))
                                      .Select(word => char.ToUpper(word[0]))
                                      .ToArray()); ;
            ToolTip = new System.Windows.Controls.ToolTip { Content = $"Open {menu.Name}" };
            MenuCommand = menu.Id;
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
