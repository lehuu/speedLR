using System.Windows;

namespace SpeedLR
{
    /// <summary>
    /// Interaction logic for PortWindow.xaml
    /// </summary>
    public partial class PortWindow : Window
    {
        public EventHandler Confirm;
        public PortWindow()
        {
            InitializeComponent();
            portTextBox.Text = LocalData.Instance.Port.ToString();
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(portTextBox.Text) && !String.Equals(portTextBox.Text, LocalData.Instance.Port.ToString()))
            {
                try
                {
                    LocalData.Instance.Port = int.Parse(portTextBox.Text);
                    LocalData.Instance.SavePort();
                    Confirm?.Invoke(this, EventArgs.Empty);
                } catch (Exception ex) {
					ErrorLogger.LogError(ex);
					Console.WriteLine($"Error parsing port: {portTextBox.Text}");
                }
            }

            Close();
        }
    }
}
