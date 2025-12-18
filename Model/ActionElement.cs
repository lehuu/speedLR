using System.Text.Json.Serialization;

namespace SpeedLR.Model
{
	public class ActionElement : MenuElement
	{
		private string _command = "";

		[JsonPropertyName("Command")]
		public string Command
		{
			get { return _command; }
			set
			{
				if (_command != value)
				{
					_command = value;
					OnPropertyChanged();
					OnPropertyChanged(nameof(Name));
				}
			}
		}

		[JsonIgnore]
		public string Name => LocalData.Instance.AvailableCommands.Categories
			.SelectMany(c => c.Commands)
			.FirstOrDefault(c => c.CommandName == _command)?.Title ?? "";
	}
}
