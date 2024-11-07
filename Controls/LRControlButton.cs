using System.Windows;

namespace SpeedLR.Controls
{
    public class LRControlButton: ControlButton
    {
        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register(
                nameof(IsActive),
                typeof(bool),
                typeof(LRControlButton),
                new PropertyMetadata(false));

        public static readonly DependencyProperty LRCommandProperty =
            DependencyProperty.Register(
                nameof(LRCommand),
                typeof(string),
                typeof(LRControlButton),
                new PropertyMetadata(""));

        public bool IsActive
        {
            get => (bool)GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }

        public string LRCommand
        {
            get => (string)GetValue(LRCommandProperty);
            set => SetValue(LRCommandProperty, value);
        }
    }
}
