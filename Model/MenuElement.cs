using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SpeedLR.Model
{
    public abstract class MenuElement
    {
		public event PropertyChangedEventHandler? PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}
}
