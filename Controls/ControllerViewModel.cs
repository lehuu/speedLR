using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using SpeedLR.Model;

namespace SpeedLR.Controls
{
    class ControllerViewModel: INotifyPropertyChanged
	{
		private Menu? _selectedMenu;
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

		private Submenu? _selectedSubmenu;
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

		private ObservableCollection<Menu> _userMenus = new ObservableCollection<Menu>();
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

		private bool _isPinned;
		public bool IsPinned
		{
			get => _isPinned;
			set
			{
				if (_isPinned != value)
				{
					_isPinned = value;
					OnPropertyChanged();
				}
			}
		}

		public enum StepMode { Single, Double, Triple }
		private StepMode _stepSize = StepMode.Single;
		public StepMode StepSize
		{
			get => _stepSize;
			set
			{
				if (_stepSize != value)
				{
					_stepSize = value;
					OnPropertyChanged();
				}
			}
		}

		public ControllerViewModel()
		{
			UserMenus = new ObservableCollection<Menu>(LocalData.Instance.UserMenus);
			SelectedMenu = UserMenus.FirstOrDefault();
		}


		public event PropertyChangedEventHandler? PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string name = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
}
