using System.IO;

namespace SpeedLR.Utils
{
	public static class ErrorLogger
	{
		private static readonly string LogFilePath =
			Path.Combine(AppContext.BaseDirectory, "error.log");

		private static bool _initialized;

		public static void Initialize()
		{
			if (_initialized) return;

			// Empty the log on each startup by recreating the file
			using (var fs = new FileStream(LogFilePath, FileMode.Create, FileAccess.Write))
			{
				// just create/truncate, nothing to write here
			}

			_initialized = true;
		}

		public static void LogError(Exception ex)
		{
			Initialize(); // ensure file is ready

			var line =
				$"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} | {ex.GetType().Name} | {ex.Message}{Environment.NewLine}{ex.StackTrace}";

			File.AppendAllText(LogFilePath, line + Environment.NewLine);
		}

		public static void LogError(string message)
		{
			Initialize();

			var line =
				$"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} | ERROR | {message}";

			File.AppendAllText(LogFilePath, line + Environment.NewLine);
		}
	}
}