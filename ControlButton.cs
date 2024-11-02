﻿using System.Windows;
using Button = System.Windows.Controls.Button;


namespace SpeedLR
{
    public class ControlButton : Button
    {
        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register(
                nameof(IsActive),
                typeof(bool),
                typeof(ControlButton),
                new PropertyMetadata(false));

        public static readonly DependencyProperty LRCommandProperty =
            DependencyProperty.Register(
                nameof(LRCommand),
                typeof(string),
                typeof(ControlButton),
                new PropertyMetadata(""));

        public bool IsActive
        {
            get => (bool)base.GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }

        public string LRCommand
        {
            get => (string)base.GetValue(LRCommandProperty);
            set => SetValue(LRCommandProperty, value);
        }
    }
}
