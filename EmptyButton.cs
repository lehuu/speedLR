using System.Windows;
using Button = System.Windows.Controls.Button;
using Brushes = System.Windows.Media.Brushes;
using System.Windows.Controls;
using SpeedLR.Model;

namespace SpeedLR
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

        public event EventHandler<MenuItemEventArg> MenuItemClick;

        public EmptyButton()
        {
            Width = 30;
            Height = 30;

            Style roundedButtonStyle = new Style(typeof(Button));

            ControlTemplate template = new ControlTemplate(typeof(Button));
            FrameworkElementFactory border = new FrameworkElementFactory(typeof(Border));
            border.Name = "border";
            border.SetValue(Border.CornerRadiusProperty, new CornerRadius(15)); // Set the desired CornerRadius here
            border.SetValue(Border.BackgroundProperty, Brushes.LightGray); // Example background color
            border.SetValue(OpacityProperty, 0.6); // Default opacity

            template.VisualTree = border;

            // Add a trigger for mouse over
            Trigger mouseOverTrigger = new Trigger
            {
                Property = IsMouseOverProperty,
                Value = true
            };
            mouseOverTrigger.Setters.Add(new Setter(Border.OpacityProperty, 1.0, "border"));

            // Add triggers to the template
            template.Triggers.Add(mouseOverTrigger);

            // Add the ControlTemplate to the style
            roundedButtonStyle.Setters.Add(new Setter(Button.TemplateProperty, template));

            Style = roundedButtonStyle;

            Background = Brushes.LightGray;

            Click += OnButtonClick;
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
                    commandItem.Click += (s, args) => {
                        var currentCommand = command;
                        MenuItemClick?.Invoke(this, new MenuItemEventArg(currentCommand));
                    };
                    categoryItem.Items.Add(commandItem);
                }

                contextMenu.Items.Add(categoryItem);
            }

            // Show the ContextMenu
            contextMenu.PlacementTarget = this;
            contextMenu.IsOpen = true;
        }
    }
}
