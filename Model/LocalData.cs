using SpeedLR.Model;
using System.Text.Json;
using System.IO;

public class LocalData
{
    private static readonly Lazy<LocalData> _instance = new Lazy<LocalData>(() => new LocalData());

    public AvailableCommands AvailableCommands { get; private set; }
    private LocalData()
    {
        AvailableCommands = LoadAvailableCommands();
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
        var filePath = "AvailableCommands.json";
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<AvailableCommands>(json) ?? new AvailableCommands();
        }
        return new AvailableCommands();
    }

}