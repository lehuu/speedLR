namespace SpeedLR.Model
{
	public class Submenu : AbstractMenu
	{
		public Submenu(string name, int position) : base(name, position)
		{

		}

		public string ShortName => string.Concat(
			Name?
				.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
				.Select(word => word[0].ToString().ToUpper()) ?? Enumerable.Empty<string>()
		);
	}
}
