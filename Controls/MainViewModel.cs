using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SpeedLR.Model;

namespace SpeedLR.Controls
{
    public class MainViewModel: INotifyPropertyChanged
	{
		private Menu _selectedMenu;

		// The list of items for the ComboBox
		public ObservableCollection<Menu> UserMenus => LocalData.Instance.UserMenus;

		// The property bound to the selected item
		public Menu SelectedMenu
		{
			get => _selectedMenu;
			set
			{
				if (_selectedMenu != value)
				{
					_selectedMenu = value;
					OnPropertyChanged();
				}
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string name = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
}
