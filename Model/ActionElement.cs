namespace SpeedLR.Model
{
	public class ActionElement : MenuElement
	{
		private string _command = "";

		public string Command
		{
			get { return _command; }
			set
			{
				if (_command != value)
				{
					_command = value;
					OnPropertyChanged();
				}
			}
		}
	}
}
