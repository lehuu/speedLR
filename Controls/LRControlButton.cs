using SpeedLR.Model;
using System.Windows;

namespace SpeedLR.Controls
{
    public class LRControlButton : ControlButton
    {
        public LRControlButton()
        {
        }

        public LRControlButton(Command command): base(ExtractTooltipContent(command))
        {
            Content = command.Short;
            LRCommand = command.CommandName;
        }

        private static string ExtractTooltipContent(Command command)
        {
            return command.Title;
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
