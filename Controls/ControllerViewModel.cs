
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

		private ActionElement _selectedAction;

		public ActionElement SelectedAction
		{
			get => _selectedAction;
			set
			{
				_selectedAction = value;
				OnPropertyChanged();
			}
		}

		public ControllerViewModel() : base()
		{
		}
	}
}
