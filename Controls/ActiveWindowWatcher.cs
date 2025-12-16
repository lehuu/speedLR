using System.Runtime.InteropServices;
using System.Text;

namespace SpeedLR.Controls
{
	public class ActiveWindowWatcher
	{
		[DllImport("user32.dll")]
		static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll")]
		static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);


		// Delegate for the WinEventProc callback function
		private delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);


		// Event constants
		private const uint EVENT_SYSTEM_FOREGROUND = 0x0003; // Event for when the foreground window changes
		private const uint WINEVENT_OUTOFCONTEXT = 0;
		private string LIGHTROOM_NAME;

		// Hook handle
		private IntPtr winEventHook;

		public ActiveWindowWatcher()
		{
			LIGHTROOM_NAME = LocalData.Instance.Environment.App;
		}

		public bool IsLightroomActive
		{
			get
			{
				if (LocalData.Instance.Environment.IsDebug)
				{
					return true;
				}
				return GetActiveWindowTitle().Contains(LIGHTROOM_NAME);
			}
		}

		private string GetActiveWindowTitle()
		{
			const int nChars = 256;
			StringBuilder buff = new StringBuilder(nChars);
			IntPtr handle = GetForegroundWindow();

			if (GetWindowText(handle, buff, nChars) > 0)
			{
				return buff.ToString();
			}
			return string.Empty;
		}

	}
}
