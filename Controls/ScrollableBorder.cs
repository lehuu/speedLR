using System.Windows.Controls;
using System.Windows.Input;

namespace SpeedLR.Controls
{
	public class ScrollableBorder : Border
	{
		// Custom Event for external subscribers
		public event MouseWheelEventHandler? Scrolled;

		protected override void OnMouseWheel(MouseWheelEventArgs e)
		{
			// 1. Raise the local event
			Scrolled?.Invoke(this, e);

			base.OnMouseWheel(e);
		}
	}
}
