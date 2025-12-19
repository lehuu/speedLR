using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace SpeedLR.Controls
{
    class ControllerViewModel: INotifyPropertyChanged
	{
		public enum StepMode { Single, Double, Triple }

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


		public event PropertyChangedEventHandler? PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string name = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
}
