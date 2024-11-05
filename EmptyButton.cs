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
            Style style = (Style)FindResource("EmptyButtonStyle");
            Style = style;


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
