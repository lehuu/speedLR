
using System.Collections.ObjectModel;
using SpeedLR.Model;

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
				_selectedSubmenu = value;
				OnPropertyChanged();
				SelectedAction = FindFirstActionItem(value?.Items);
			}
		}

		public ControllerViewModel() : base()
		{
			SelectedAction = FindFirstActionItem(SelectedSubmenu?.Items);
		}

		public ActionElement? FindFirstActionItem(ObservableCollection<MenuElement>? list) =>
			list?.OfType<ActionElement>().FirstOrDefault();
	}
}
