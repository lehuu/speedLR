using System.Windows;
using SpeedLR.Utils;

namespace SpeedLR
{
	/// <summary>
	/// Interaction logic for EditMenuWindow.xaml
	/// </summary>
	public partial class EditMenuWindow : Window
	{
		public string ResultName { get; private set; }
		public EditMenuWindow(String menuname)
		{
			InitializeComponent();
			nameTextBox.Text = menuname;

			Title = String.IsNullOrEmpty(menuname) ? "New Menu" : "Edit Menu";
		}
		public EditMenuWindow() : this("")
		{
		}

		private void ConfirmButton_Click(object sender, RoutedEventArgs e)
		{
			if (!String.IsNullOrEmpty(nameTextBox.Text))
			{
				ResultName = nameTextBox.Text;
				this.DialogResult = true;
			}
			else
			{
				this.DialogResult = false;
			}
		}
	}
}
