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
		public ObservableCollection<Menu> UserMenus { get; set; }

		public MainViewModel()
		{
			UserMenus = new ObservableCollection<Menu>(LocalData.Instance.UserMenus);
		}

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

		public void SaveMenus()
		{
			LocalData.Instance.UserMenus = UserMenus.OrderBy(m => m.Position).ToList();
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

		private void UpdatePositionProperties()
		{
			for (int i = 0; i < UserMenus.Count; i++)
			{
				UserMenus[i].Position = i;
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string name = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
}
