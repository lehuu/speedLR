using System.Windows;
using Button = System.Windows.Controls.Button;
using Brushes = System.Windows.Media.Brushes;
using System.Windows.Controls;
using SpeedLR.Model;
using System.Windows.Media;

namespace SpeedLR.Controls
{
    class EmptyButton : Button
    {
        public class MenuItemEventArg : EventArgs
        {
            public Command Value { get; set; }

            public MenuItemEventArg(Command value)
            {
                Value = value;
            }
        }

        public enum ColorType
        {
            Background,
            Font
        }

        public class ColorItemEventArg : EventArgs
        {
            public string Value { get; set; }
            public ColorType Type { get; set; }

            public ColorItemEventArg(string value, ColorType type)
            {
                Value = value;
                Type = type;
            }
        }

        public event EventHandler<MenuItemEventArg> MenuItemClick;
        public event EventHandler<ColorItemEventArg> ColorItemClick;
        public event EventHandler<string> SubmenuItemClick;
        public event EventHandler ClearClick;

        private Command? _command;
        public Command? Command
        {
            get
            {
                return _command;
            }
            set
            {
                _command = value;

                if (value == null)
                {
                    ToolTip = null;
                    Content = null;
                    return;
                }

                _submenu = null;
                Content = value.Short;
                ToolTip = new System.Windows.Controls.ToolTip { Content = value.Title };
            }
        }

        private Model.Menu? _submenu;
        public Model.Menu? Submenu
        {
            get
            {
                return _submenu;
            }
            set
            {
                _submenu = value;

                if (value == null)
                {
                    ToolTip = null;
                    Content = null;
                    return;
                }

                _command = null;
                Content = new string(value.Name.Split(' ')
                                      .Where(word => !string.IsNullOrEmpty(word))
                                      .Select(word => char.ToUpper(word[0]))
                                      .ToArray());
                ToolTip = new System.Windows.Controls.ToolTip { Content = value.Name };
            }
        }

        public EmptyButton()
        {
            Style style = (Style)FindResource("EmptyButtonStyle");
            Style = style;


            ToolTipService.SetInitialShowDelay(this, 0);
            ToolTipService.SetShowDuration(this, int.MaxValue);

            Click += OnButtonClick;
        }

        public void ResetEventsHandlers()
        {
            MenuItemClick = null;
            ColorItemClick = null;
            ClearClick = null;
            SubmenuItemClick = null;
        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            // Create a new ContextMenu
            ContextMenu contextMenu = new ContextMenu();

            // Populate the ContextMenu with categories and commands
            foreach (var category in LocalData.Instance.AvailableCommands.Categories)
            {
                MenuItem categoryItem = new MenuItem { Header = category.Title };

                foreach (var command in category.Commands)
                {
                    MenuItem commandItem = new MenuItem { Header = command.Title };
                    commandItem.Click += (s, args) =>
                    {
                        var currentCommand = command;
                        Command = command;
                        MenuItemClick?.Invoke(this, new MenuItemEventArg(currentCommand));
                    };
                    categoryItem.Items.Add(commandItem);
                }

                contextMenu.Items.Add(categoryItem);
            }

            MenuItem menuItem = new MenuItem { Header = "Open Menu", Background = Brushes.LightSlateGray };

            foreach (var item in LocalData.Instance.AvailableMenus.Menus)
            {
                var submenuItem = new MenuItem { Header = item.Name };
                submenuItem.Click += (s, args) =>
                {
                    Submenu = item;
                    SubmenuItemClick?.Invoke(this, item.Id);
                };

                menuItem.Items.Add(submenuItem);
            }

            contextMenu.Items.Add(menuItem);

            MenuItem fontColorItem = new MenuItem { Header = "Font Color", Background = Brushes.DarkGray };
            MenuItem backgroundItem = new MenuItem { Header = "Background Color", Background = Brushes.DarkGray };


            foreach (var colorGroup in ColorData.GetColorGroups())
            {
                fontColorItem.Items.Add(CreateColorGroupMenuItem(colorGroup, ColorType.Font));
            }

            foreach (var colorGroup in ColorData.GetColorGroups())
            {
                backgroundItem.Items.Add(CreateColorGroupMenuItem(colorGroup, ColorType.Background));
            }

            contextMenu.Items.Add(backgroundItem);
            contextMenu.Items.Add(fontColorItem);


            MenuItem deleteItem = new MenuItem { Header = "Delete", Background = Brushes.IndianRed };
            deleteItem.Click += (s, args) =>
            {
                Command = null;
                Background = BrushHelper.GetBrushFromHex(ColorData.DEFAULT_BACKGROUND);
                Foreground = BrushHelper.GetBrushFromHex(ColorData.DEFAULT_FONT);
                ClearClick?.Invoke(this, EventArgs.Empty);
            };

            contextMenu.Items.Add(deleteItem);

            // Show the ContextMenu
            contextMenu.PlacementTarget = this;
            contextMenu.IsOpen = true;
        }

        private MenuItem CreateColorGroupMenuItem(ColorGroup colorGroup, ColorType type)
        {
            MenuItem menuItem = new MenuItem { Header = colorGroup.Name };

            foreach (var hex in colorGroup.ColorHexes)
            {
                MenuItem colorItem = new MenuItem
                {
                    Header = $"{colorGroup.Name} {Array.IndexOf(colorGroup.ColorHexes.ToArray(), hex) * 100 + 50}",
                    Background = (SolidColorBrush)new BrushConverter().ConvertFromString(hex),
                };

                colorItem.Click += (s, args) =>
                {
                    switch (type)
                    {
                        case ColorType.Background:
                            Background = BrushHelper.GetBrushFromHex(hex);
                            break;
                        case ColorType.Font:
                            Foreground = BrushHelper.GetBrushFromHex(hex);
                            break;
                    }

                    ColorItemClick?.Invoke(this, new ColorItemEventArg(hex, type));
                };

                menuItem.Items.Add(colorItem);
            }

            return menuItem;
        }
    }
}
