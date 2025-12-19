using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SpeedLR.Controls
{
    class ControllerViewModel: INotifyPropertyChanged
	{
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


		public event PropertyChangedEventHandler? PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string name = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
}
