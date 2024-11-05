using SpeedLR.Model;
using System.Text.Json;
using System.IO;

public class LocalData
{
    private static readonly Lazy<LocalData> _instance = new Lazy<LocalData>(() => new LocalData());

    private string COMMAND_PATH = "AvailableCommands.json";
    private string MENU_PATH = "MyMenus.json";
    public AvailableCommands AvailableCommands { get; private set; }

    public AvailableMenus AvailableMenus { get; set; }

    private LocalData()
    {
        AvailableCommands = LoadAvailableCommands();
        AvailableMenus = LoadAvailableMenus();
    }

    public static LocalData Instance
    {
        get
        {
            return _instance.Value;
        }
    }

    private AvailableCommands LoadAvailableCommands()
    {
        if (File.Exists(COMMAND_PATH))
        {
            string json = File.ReadAllText(COMMAND_PATH);
            return JsonSerializer.Deserialize<AvailableCommands>(json) ?? new AvailableCommands();
        }
        return new AvailableCommands();
    }

    private AvailableMenus LoadAvailableMenus()
    {
        if (File.Exists(MENU_PATH))
        {
            string json = File.ReadAllText(MENU_PATH);
            return JsonSerializer.Deserialize<AvailableMenus>(json) ?? new AvailableMenus();
        }
        return new AvailableMenus();
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
            Console.WriteLine($"Error writing {MENU_PATH}: {ex.Message}");
        }
    }

}