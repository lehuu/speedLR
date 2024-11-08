using SpeedLR.Model;
using System.Windows;

namespace SpeedLR.Controls
{
    public class LRControlButton: ControlButton
    {
        public LRControlButton()
        {
        }

        public LRControlButton(Command command)
        {
            Content = command.Short;
            ToolTip = new System.Windows.Controls.ToolTip { Content = command.Title };
        }

        public static readonly DependencyProperty LRCommandProperty =
            DependencyProperty.Register(
                nameof(LRCommand),
                typeof(string),
                typeof(LRControlButton),
                new PropertyMetadata(""));

        public string LRCommand
        {
            get => (string)GetValue(LRCommandProperty);
            set => SetValue(LRCommandProperty, value);
        }
    }
}
