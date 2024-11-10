using System.Windows;
using System.Windows.Controls;
using Button = System.Windows.Controls.Button;
using ToolTip = System.Windows.Controls.ToolTip;

namespace SpeedLR.Controls
{
    public abstract class ControlButton : Button
    {
        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register(
                nameof(IsActive),
                typeof(bool),
                typeof(ControlButton),
                new PropertyMetadata(false));

        public bool IsActive
        {
            get => (bool)GetValue(IsActiveProperty);
            set  { SetValue(IsActiveProperty, value);
                if(ToolTip != null && ToolTip is ToolTip tooltip && tooltip.Content != null)
                {
                    tooltip.IsOpen = value;
                }
            }
        }

        public ControlButton()
        {
            ToolTipService.SetInitialShowDelay(this, 0);
            Unloaded += ControlButton_Unloaded;
        }
        private void ControlButton_Unloaded(object sender, RoutedEventArgs e)
        {
            // Clear the ToolTip to remove it from the visual tree
            ToolTip = null;
        }
    }
}
