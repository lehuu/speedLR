using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SpeedLR.Controls
{
	public class ScrollableBorder : Border
	{
		private int _scrollAccumulator = 0;
		private bool _isHovered = false;
		private LowLevelHotkey[] _hotkeys;

		public static readonly DependencyProperty ScrollThresholdProperty =
			DependencyProperty.Register(nameof(ScrollThreshold), typeof(int), typeof(ScrollableBorder), new PropertyMetadata(120));

		public ScrollableBorder()
		{
			this.Loaded += (s, e) =>
			{
				Window parentWindow = Window.GetWindow(this);
				if (parentWindow != null)
				{
					parentWindow.Closed += ParentWindow_Closed;
				}
			};

			_hotkeys = new LowLevelHotkey[]
			{
					CreateHotkeyPress(Key.Back, (ref bool isHandled) =>
					{
						if(_isHovered && ResetKey != null) {
							ResetKey.Invoke();
							isHandled = true;
						}
					}),
					CreateHotkeyPress(Key.Left, (ref bool isHandled) =>
					{
						if(_isHovered && LeftRight != null) {
							LeftRight.Invoke(-1);
							isHandled = true;
						}
					}),
					CreateHotkeyPress(Key.Right, (ref bool isHandled) =>
					{
						if(_isHovered && LeftRight != null) {
							LeftRight.Invoke(1);
							isHandled = true;
						}
					}),
					CreateHotkeyPress(Key.Up, (ref bool isHandled) =>
					{
						if(_isHovered && UpDown != null) {
							UpDown.Invoke(1);
							isHandled = true;
						}
					}),
					CreateHotkeyPress(Key.Down, (ref bool isHandled) =>
					{
						if(_isHovered && UpDown != null) {
							UpDown.Invoke(-1);
							isHandled = true;
						}
					}),
					CreateHotkeyPress(Key.Left, Key.LeftCtrl, (ref bool isHandled) =>
					{
						if(_isHovered && CtrlLeftRight != null) {
							CtrlLeftRight.Invoke(-1);
							isHandled = true;
						}
					}),
					CreateHotkeyPress(Key.Right, Key.LeftCtrl, (ref bool isHandled) =>
					{
						if(_isHovered && CtrlLeftRight != null) {
							CtrlLeftRight.Invoke(1);
							isHandled = true;
						}
					}),
					CreateHotkeyPress(Key.Up, Key.LeftCtrl, (ref bool isHandled) =>
					{
						if(_isHovered && CtrlUpDown != null) {
							CtrlUpDown.Invoke(1);
							isHandled = true;
						}
					}),
					CreateHotkeyPress(Key.Down, Key.LeftCtrl, (ref bool isHandled) =>
					{
						if(_isHovered && CtrlUpDown != null) {
							CtrlUpDown.Invoke(-1);
							isHandled = true;
						}
					}),
					CreateHotkeyPress(Key.Left, Key.LeftAlt, (ref bool isHandled) =>
					{
						if(_isHovered && AltLeftRight != null) {
							AltLeftRight.Invoke(-1);
							isHandled = true;
						}
					}),
					CreateHotkeyPress(Key.Right, Key.LeftAlt, (ref bool isHandled) =>
					{
						if(_isHovered && AltLeftRight != null) {
							AltLeftRight.Invoke(1);
							isHandled = true;
						}
					}),
			};
		}

		private LowLevelHotkey CreateHotkeyPress(Key key, Key modifier, ActionRef clickEvent)
		{
			var result = new LowLevelHotkey(key, modifier);
			result.KeyPressed += clickEvent;
			return result;
		}

		private LowLevelHotkey CreateHotkeyPress(Key key, ActionRef clickEvent)
		{
			return CreateHotkeyPress(key, Key.None, clickEvent);
		}

		public int ScrollThreshold
		{
			get => (int)GetValue(ScrollThresholdProperty);
			set => SetValue(ScrollThresholdProperty, value);
		}

		public event Action<int>? ScrollStep;
		protected override void OnMouseWheel(MouseWheelEventArgs e)
		{
			if (ScrollStep == null)
			{
				base.OnMouseWheel(e);
				return;
			}

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
			if (MiddleClick == null)
			{
				base.OnMouseDown(e);
				return;
			}
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
			_isHovered = true;
		}

		protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e)
		{
			base.OnMouseLeave(e);
			_isHovered = false;
		}

		public event Action<int>? UpDown;
		public event Action<int>? CtrlUpDown;
		public event Action<int>? LeftRight;
		public event Action<int>? CtrlLeftRight;
		public event Action<int>? AltLeftRight;
		public event Action? ResetKey;

		private void ParentWindow_Closed(object sender, EventArgs e)
		{
			if (sender is Window w) w.Closed -= ParentWindow_Closed;

			if (_hotkeys != null)
			{
				foreach (var key in _hotkeys)
				{
					key.Dispose();
				}
			}
		}
	}
}