using System.Windows;
using System.Windows.Controls;
using Button = System.Windows.Controls.Button;

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
            set
            {
                SetValue(IsActiveProperty, value);
            }
        }

        public string? PopupText
        {
            get; set;
        }

        public ControlButton()
        {
            ToolTipService.SetInitialShowDelay(this, 0);
        }

        public ControlButton(string popupText) : this()
        {
            VerticalAlignment = VerticalAlignment.Top;
            PopupText = popupText;
            ToolTip = new System.Windows.Controls.ToolTip
            {
                Content = popupText,
            };
        }
    }
}
