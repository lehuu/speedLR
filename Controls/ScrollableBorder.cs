using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SpeedLR.Controls
{
	public class ScrollableBorder : Border
	{
		private int _scrollAccumulator = 0;

		public static readonly DependencyProperty ScrollThresholdProperty =
			DependencyProperty.Register(nameof(ScrollThreshold), typeof(int), typeof(ScrollableBorder), new PropertyMetadata(120));

		public ScrollableBorder()
		{
			Focusable = true;
		}

		public int ScrollThreshold
		{
			get => (int)GetValue(ScrollThresholdProperty);
			set => SetValue(ScrollThresholdProperty, value);
		}

		public event Action<int>? ScrollStep;
		protected override void OnMouseWheel(MouseWheelEventArgs e)
		{
			// 1. Detect if the input is from a trackpad (Precision Scrolling)
			// A delta that isn't a multiple of 120 is the most accurate 
			// indicator of a precision touch device/trackpad in WPF.
			bool isTrackpad = e.Delta % 120 != 0;

			// 2. Handle direction reset on change
			if ((e.Delta > 0 && _scrollAccumulator < 0) || (e.Delta < 0 && _scrollAccumulator > 0))
			{
				_scrollAccumulator = 0;
			}

			_scrollAccumulator += e.Delta;

			int threshold = ScrollThreshold;
			while (Math.Abs(_scrollAccumulator) >= threshold)
			{
				// Calculate base direction: Up (1), Down (-1)
				int direction = _scrollAccumulator > 0 ? 1 : -1;

				// 3. Apply Inversion logic for Trackpads
				// If trackpad: Scrolling down finger-wise usually results in negative delta, 
				// but we invert it here to match natural scroll expectations.
				if (isTrackpad)
				{
					direction *= -1;
				}

				ScrollStep?.Invoke(direction);

				if (_scrollAccumulator > 0) _scrollAccumulator -= threshold;
				else _scrollAccumulator += threshold;
			}

			e.Handled = true;
			base.OnMouseWheel(e);
		}

		public event Action? MiddleClick;

		protected override void OnMouseDown(MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Middle)
			{
				MiddleClick?.Invoke();
				e.Handled = true; // Prevents the event from bubbling up
			}
			base.OnMouseDown(e);
		}

		protected override void OnMouseEnter(System.Windows.Input.MouseEventArgs e)
		{
			base.OnMouseEnter(e);
			// Take focus automatically when the mouse enters the area
			this.Focus();
		}

		protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e)
		{
			base.OnMouseLeave(e);
			// Release focus when the mouse leaves so other controls can work
			Keyboard.ClearFocus();
		}

		public event Action<int>? UpDown;
		public event Action<int>? LeftRight;
		public event Action? ResetKey;

		protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
		{
			// Only respond if the mouse is actually over the element
			if (!IsMouseOver) return;

			switch (e.Key)
			{
				case Key.Up:
					UpDown?.Invoke(1);
					e.Handled = true;
					break;
				case Key.Down:
					UpDown?.Invoke(-1);
					e.Handled = true;
					break;
				case Key.Left:
					LeftRight?.Invoke(-1);
					e.Handled = true;
					break;
				case Key.Right:
					LeftRight?.Invoke(1);
					e.Handled = true;
					break;
				case Key.Back:
					ResetKey?.Invoke();
					e.Handled = true;
					break;
			}

			base.OnKeyDown(e);
		}
	}
}