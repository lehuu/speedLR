using System.Windows;
using SpeedLR.Utils;

namespace SpeedLR
{
    /// <summary>
    /// Interaction logic for PortWindow.xaml
    /// </summary>
    public partial class PortWindow : Window
    {
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
					this.DialogResult = true;
				}
				catch (Exception ex) {
					ErrorLogger.LogError(ex);
					Console.WriteLine($"Error parsing port: {portTextBox.Text}");
                }
			}
            else
            {
				this.DialogResult = false;
			}
		}
	}
}
