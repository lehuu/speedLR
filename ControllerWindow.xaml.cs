using System.Windows;

namespace SpeedLR
{
    /// <summary>
    /// Interaction logic for ControllerWindow.xaml
    /// </summary>
    public partial class ControllerWindow : Window
    {
        public ControllerWindow()
        {
            InitializeComponent();
        }
        private void ControllerWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
    }
}
