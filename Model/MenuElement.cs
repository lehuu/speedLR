using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace SpeedLR.Model
{
	[JsonDerivedType(typeof(ActionElement), typeDiscriminator: "action")]
	[JsonDerivedType(typeof(SeparatorElement), typeDiscriminator: "separator")]
	public abstract class MenuElement
    {
		public event PropertyChangedEventHandler? PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}
}
