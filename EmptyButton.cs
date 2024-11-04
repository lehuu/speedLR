using System.Windows;
using Button = System.Windows.Controls.Button;
using Brushes = System.Windows.Media.Brushes;
using System.Windows.Controls;

namespace SpeedLR
{
    class EmptyButton: Button
    {
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
        }
    }
}
