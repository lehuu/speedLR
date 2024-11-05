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

                if(value == null)
                {
                    ToolTip = null;
                    Content = null;
                    return;
                }

                Content = value.Short;
                ToolTip = new System.Windows.Controls.ToolTip { Content = value.Title };
            }
        }

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

            FrameworkElementFactory contentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
            contentPresenter.SetValue(HorizontalAlignmentProperty, System.Windows.HorizontalAlignment.Center);
            contentPresenter.SetValue(VerticalAlignmentProperty, VerticalAlignment.Center);
            contentPresenter.SetValue(ForegroundProperty, Brushes.Black); // Set the desired font color here
            border.AppendChild(contentPresenter);

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

            ToolTipService.SetInitialShowDelay(this, 0); // Show immediately
            ToolTipService.SetShowDuration(this, int.MaxValue);

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

            MenuItem deleteItem = new MenuItem { Header = "Delete", Background = Brushes.IndianRed };
            deleteItem.Click += (s, args) =>
            {
                Command = null;
                ClearClick?.Invoke(this, EventArgs.Empty);
            };

            contextMenu.Items.Add(deleteItem);

            // Show the ContextMenu
            contextMenu.PlacementTarget = this;
            contextMenu.IsOpen = true;
        }
    }
}
