using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SpeedLR.Model;

namespace SpeedLR.Controls
{
	public class MainViewModel : INotifyPropertyChanged
	{
		private Menu? _selectedMenu;
		private Submenu? _selectedSubmenu;
		private ObservableCollection<Menu> _userMenus = new ObservableCollection<Menu>();

		public MainViewModel()
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

		public void SaveMenus()
		{
			UserMenus = new ObservableCollection<Menu>(UserMenus.OrderBy(m => m.Position));
			UpdatePositionProperties();
			LocalData.Instance.UserMenus = UserMenus.ToList();
			LocalData.Instance.SaveUserMenus();
		}

		public void MoveSelectedMenu(bool moveUp)
		{
			if (SelectedMenu == null) return;

			int oldIndex = UserMenus.IndexOf(SelectedMenu);
			int newIndex = moveUp ? oldIndex - 1 : oldIndex + 1;

			if (newIndex < 0 || newIndex >= UserMenus.Count) return;

			UserMenus.Move(oldIndex, newIndex);

			UpdatePositionProperties();
			SaveMenus();
		}

		public void MoveSubmenu(Submenu submenu, bool moveUp)
		{
			if (SelectedMenu == null) return;

			int oldIndex = SelectedMenu.Submenus.IndexOf(submenu);
			int newIndex = moveUp ? oldIndex - 1 : oldIndex + 1;

			if (newIndex < 0 || newIndex >= SelectedMenu.Submenus.Count) return;

			SelectedMenu.Submenus.Move(oldIndex, newIndex);

			UpdatePositionProperties();
			SaveMenus();
		}

		private void UpdatePositionProperties()
		{
			for (int i = 0; i < UserMenus.Count; i++)
			{
				UserMenus[i].Position = i;
				for (int j = 0; j < UserMenus[i].Submenus.Count; j++)
				{
					UserMenus[i].Submenus[j].Position = j;

					for (int x = 0; x < UserMenus[i].Submenus[j].Items.Count; x++)
					{
						UserMenus[i].Submenus[j].Items[x].Position = x;
					}
				}
			}
		}

		public void EditMenuElement(MenuElement element)
		{
			if (SelectedSubmenu == null)
			{
				return;
			}

			int index = SelectedSubmenu.Items.IndexOf(element);
			if (index != -1)
			{
				// Remove and Re-insert is the "nuclear option" to force 
				// WPF DataTemplates to completely re-render the control.
				SelectedSubmenu.Items.RemoveAt(index);
				SelectedSubmenu.Items.Insert(index, element);
			}
			else
			{
				SelectedSubmenu.Items.Add(element);
			}

			SaveMenus();
		}

		public void MoveMenuItem(MenuElement submenu, bool moveUp)
		{
			if (SelectedSubmenu == null) return;

			int oldIndex = SelectedSubmenu.Items.IndexOf(submenu);
			int newIndex = moveUp ? oldIndex - 1 : oldIndex + 1;

			if (newIndex < 0 || newIndex >= SelectedSubmenu.Items.Count) return;

			SelectedSubmenu.Items.Move(oldIndex, newIndex);

			UpdatePositionProperties();
			SaveMenus();
		}

		public event PropertyChangedEventHandler? PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string name = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
}
