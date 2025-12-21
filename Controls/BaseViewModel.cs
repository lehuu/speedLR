using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SpeedLR.Model;

namespace SpeedLR.Controls
{
	public class BaseViewModel : INotifyPropertyChanged
	{
		protected Menu? _selectedMenu;
		protected Submenu? _selectedSubmenu;
		protected ObservableCollection<Menu> _userMenus = new ObservableCollection<Menu>();

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

		public virtual Submenu? SelectedSubmenu
		{
			get => _selectedSubmenu;
			set
			{
				_selectedSubmenu = value;
				OnPropertyChanged();
			}
		}

		public Menu? SelectedMenu
		{
			get => _selectedMenu;
			set
			{
				_selectedMenu = value;
				OnPropertyChanged();
				SelectedSubmenu = value?.Submenus.FirstOrDefault();
			}
		}

		public ObservableCollection<Menu> UserMenus
		{
			get => _userMenus;
			set
			{
				_userMenus = value;
				OnPropertyChanged();
			}
		}

	}
}
