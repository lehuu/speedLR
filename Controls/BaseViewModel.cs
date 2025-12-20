using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SpeedLR.Model;

namespace SpeedLR.Controls
{
	public class BaseViewModel : INotifyPropertyChanged
	{
		private Menu? _selectedMenu;
		private Submenu? _selectedSubmenu;
		private ObservableCollection<Menu> _userMenus = new ObservableCollection<Menu>();

		public event PropertyChangedEventHandler? PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string name = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		public BaseViewModel()
		{
			UserMenus = new ObservableCollection<Menu>(LocalData.Instance.UserMenus);
			SelectedMenu = UserMenus.FirstOrDefault();
		}

		public Submenu? SelectedSubmenu
		{
			get => _selectedSubmenu;
			set
			{
				if (_selectedSubmenu != value)
				{
					_selectedSubmenu = value;
					OnPropertyChanged();
				}
			}
		}

		public Menu? SelectedMenu
		{
			get => _selectedMenu;
			set
			{
				if (_selectedMenu != value)
				{
					_selectedMenu = value;
					SelectedSubmenu = value?.Submenus.FirstOrDefault();
					OnPropertyChanged();
				}
			}
		}

		public ObservableCollection<Menu> UserMenus
		{
			get => _userMenus;
			set
			{
				if (_userMenus != value)
				{
					_userMenus = value;
					OnPropertyChanged();
				}
			}
		}

	}
}
