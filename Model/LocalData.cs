using System.IO;
using System.Text.Json;
using SpeedLR;
using SpeedLR.Model;

public class LocalData
{
    private static readonly Lazy<LocalData> _instance = new Lazy<LocalData>(() => new LocalData());

    private string COMMAND_PATH = "AvailableCommands.json";
    private string MENU_PATH = "MyMenus.json";
    private string ENV_PATH = "env";
    private string PORT_PATH = "Port.txt";
	public AvailableCommands AvailableCommands { get; private set; }
    
    public PluginEnvironment Environment { get; private set; }

    public AvailableMenus AvailableMenus { get; set; }
    public int Port { get; set; }

    private LocalData()
    {
		Environment = LoadEnvironment();
        AvailableCommands = LoadAvailableCommands();
		AvailableMenus = LoadAvailableMenus();
        Port = LoadPort();
    }

    public static LocalData Instance
    {
        get
        {
            return _instance.Value;
        }
    }

	private PluginEnvironment LoadEnvironment()
	{
		if (File.Exists(ENV_PATH))
		{
			string json = File.ReadAllText(ENV_PATH);
			try
			{
                var x = JsonSerializer.Deserialize<PluginEnvironment>(json);
				return JsonSerializer.Deserialize<PluginEnvironment>(json) ?? new PluginEnvironment();
			}
			catch (Exception ex)
			{
				ErrorLogger.LogError(ex);
				return new PluginEnvironment();
			}
		}
		return new PluginEnvironment();
	}

	private AvailableCommands LoadAvailableCommands()
    {
        if (File.Exists(COMMAND_PATH))
        {
            string json = File.ReadAllText(COMMAND_PATH);
            try
            {
                return JsonSerializer.Deserialize<AvailableCommands>(json) ?? new AvailableCommands();
            }
            catch (Exception ex)
            {
				ErrorLogger.LogError(ex);
				return new AvailableCommands();
            }
        }
        return new AvailableCommands();
    }

    private int LoadPort()
    {
        if (File.Exists(PORT_PATH))
        {
            try
            {
                int result = int.Parse(File.ReadAllText(PORT_PATH));
                return result;
            }
            catch (Exception ex)
            {
				ErrorLogger.LogError(ex);
				return 49000;
            }
        }
        return 49000;
    }

    private AvailableMenus LoadAvailableMenus()
    {
        if (File.Exists(MENU_PATH))
        {
            string json = File.ReadAllText(MENU_PATH);
            try
            {
                return JsonSerializer.Deserialize<AvailableMenus>(json) ?? new AvailableMenus();
            }
            catch (Exception ex)
            {
				ErrorLogger.LogError(ex);
				return new AvailableMenus();
            }
        }
        return new AvailableMenus();
    }

    public void SavePort()
    {
        try
        {
            File.WriteAllText(PORT_PATH, Port.ToString());
        }
        catch (Exception ex)
        {
			ErrorLogger.LogError(ex);
			Console.WriteLine($"Error writing {PORT_PATH}: {ex.Message}");
        }
    }
    public void SaveAvailableMenus()
    {
        try
        {
            string json = JsonSerializer.Serialize(AvailableMenus);
            File.WriteAllText(MENU_PATH, json);
        }
        catch (Exception ex)
        {
			ErrorLogger.LogError(ex);
			Console.WriteLine($"Error writing {MENU_PATH}: {ex.Message}");
        }
    }

}