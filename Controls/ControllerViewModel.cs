
using System.Collections.ObjectModel;
using SpeedLR.Model;
using SpeedLR.Utils;

namespace SpeedLR.Controls
{
	public class ControllerViewModel : BaseViewModel
	{
		private bool _isPinned;
		public bool IsPinned
		{
			get => _isPinned;
			set
			{
				_isPinned = value;
				OnPropertyChanged();
			}
		}

		public Dictionary<string, string> DefaultSubmenuMap { get; set; } = new Dictionary<string, string>();

		public bool IsConnected => Connector.Instance.Status == Connector.ConnectionStatus.CONNECTED;

		public enum StepMode { Single, Double, Triple }
		private StepMode _stepSize = StepMode.Single;
		public StepMode StepSize
		{
			get => _stepSize;
			set
			{
				_stepSize = value;
				OnPropertyChanged();
			}
		}

		private ActionElement? _selectedAction;

		public ActionElement? SelectedAction
		{
			get => _selectedAction;
			set
			{
				_selectedAction = value;
				OnPropertyChanged();
			}
		}

		public override Submenu? SelectedSubmenu
		{
			get => _selectedSubmenu;
			set
			{
				SelectedAction = FindDefaultActionItem(_selectedSubmenu?.Items ?? new ObservableCollection<MenuElement>(), value?.Items ?? new ObservableCollection<MenuElement>());
				_selectedSubmenu = value;

				if (value == null)
				{
					DefaultSubmenuMap.Remove(SelectedMenu?.Id ?? "");
				}
				else
				{
					DefaultSubmenuMap[SelectedMenu?.Id ?? ""] = value.Id;
				}

				OnPropertyChanged();
			}
		}

		public override Menu? SelectedMenu
		{
			get => _selectedMenu;
			set
			{
				_selectedMenu = value;
				OnPropertyChanged();

				if (value != null && DefaultSubmenuMap.ContainsKey(value.Id))
				{
					SelectedSubmenu = value?.Submenus.FirstOrDefault(s => s.Id == DefaultSubmenuMap[value.Id]);
				}
				else
				{
					SelectedSubmenu = value?.Submenus.FirstOrDefault();
				}
			}
		}

		public ControllerViewModel() : base()
		{
			SelectedAction = SelectedSubmenu?.Items.OfType<ActionElement>().FirstOrDefault();
			Connector.Instance.ConnectionChanged += (s, e) => OnPropertyChanged(nameof(IsConnected));
		}

		public ActionElement? FindDefaultActionItem(ObservableCollection<MenuElement> oldList, ObservableCollection<MenuElement> newList)
		{
			if (SelectedAction == null || newList.Count == 0)
			{
				return newList.OfType<ActionElement>().FirstOrDefault();
			}

			var oldActionList = oldList.OfType<ActionElement>().ToList();
			var newActionList = newList.OfType<ActionElement>().ToList();

			var oldPosition = oldActionList.IndexOf(SelectedAction);

			var normalizedIndex = Math.Max(0, Math.Min(newActionList.Count - 1, oldPosition));

			return newActionList[normalizedIndex];
		}
	}
}
