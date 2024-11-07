using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SpeedLR.Controls
{
    public class MenuControlButton: ControlButton
    {
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
