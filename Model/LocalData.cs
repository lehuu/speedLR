using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using SpeedLR.Model;
using SpeedLR.Utils;
public interface ILocalData
{
	AvailableCommands AvailableCommands { get; }
	PluginEnvironment Environment { get; }
	AvailableMenus AvailableMenus { get; set; }
	List<Menu> UserMenus { get; set; }
	int Port { get; set; }

	void SavePort();
	void SaveUserMenus();
}

public sealed class LocalData : ILocalData
{
	// Use private static fields for configuration constants
	private const string CommandPath = "AvailableCommands.json";
	private const string LegacyMenuPath = "LegacyMenus.json";
	private const string MenuPath = "UserMenus.json";
	private const string EnvPath = "env";
	private const string PortPath = "port";
	private const int DefaultPort = 49000;

	// Standard JsonSerializerOptions, making it easier to manage settings
	private static readonly JsonSerializerOptions SerializerOptions = new()
	{
		// For writing files, make them readable
		WriteIndented = true,
		// Handles enums in a more robust way
		Converters = { new JsonStringEnumConverter() }
	};

	// --- Singleton Implementation ---

	// The Lazy initializer pattern you used is correct for thread-safe lazy instantiation.
	private static readonly Lazy<LocalData> _instance = new(() => new LocalData());

	// Public access property using expression body (C# 6+)
	public static LocalData Instance => _instance.Value;

	// Private constructor to enforce Singleton pattern
	private LocalData()
	{
		// Load all data upon instantiation
		Environment = LoadDataFromFile<PluginEnvironment>(EnvPath, new PluginEnvironment());
		AvailableCommands = LoadDataFromFile<AvailableCommands>(CommandPath, new AvailableCommands());
		AvailableMenus = LoadDataFromFile<AvailableMenus>(LegacyMenuPath, new AvailableMenus());
		UserMenus = LoadDataFromFile<List<Menu>>(MenuPath, new List<Menu>()).OrderBy(m => m.Position).ToList();

		Port = LoadPort();
	}

	// --- Public Properties ---

	public AvailableCommands AvailableCommands { get; }
	public PluginEnvironment Environment { get; }
	public AvailableMenus AvailableMenus { get; set; }
	public List<Menu> UserMenus { get; set; }

	public int Port { get; set; }

	// --- Private Loading Logic (Generic) ---

	// A generic method to consolidate the repetitive JSON loading logic
	private static T LoadDataFromFile<T>(string filePath, T defaultInstance) where T : new()
	{
		if (!File.Exists(filePath))
		{
			return defaultInstance;
		}

		try
		{
			// Use File.ReadAllText for simplicity with small configuration files
			string json = File.ReadAllText(filePath);

			// Return deserialized object or the provided default if deserialization fails (null coalescing)
			return JsonSerializer.Deserialize<T>(json, SerializerOptions) ?? defaultInstance;
		}
		catch (Exception ex)
		{
			// Log the error using the external logger
			ErrorLogger.LogError(ex);
			// Fallback to the default instance on error
			return defaultInstance;
		}
	}

	private int LoadPort()
	{
		if (!File.Exists(PortPath))
		{
			return DefaultPort;
		}

		try
		{
			// Use Trim() to handle potential whitespace in the text file
			string text = File.ReadAllText(PortPath).Trim();
			// TryParse is safer than Parse, but since we are catching exceptions anyway, Parse is acceptable here.
			// Using int.Parse for direct exception handling as in the original code.
			return int.Parse(text);
		}
		catch (Exception ex)
		{
			ErrorLogger.LogError(ex);
			// It's helpful to also print to console for immediate debugging during development
			Console.WriteLine($"Error reading {PortPath}: {ex.Message}");
			return DefaultPort;
		}
	}

	// --- Public Saving Logic ---

	public void SavePort()
	{
		try
		{
			// Using File.WriteAllText is fine for simple integer port
			File.WriteAllText(PortPath, Port.ToString());
		}
		catch (Exception ex)
		{
			ErrorLogger.LogError(ex);
			Console.WriteLine($"Error writing {PortPath}: {ex.Message}");
		}
	}

	public void SaveUserMenus()
	{
		// Use a generic save helper to reduce duplication, if more save methods were needed.
		SaveDataToFile(MenuPath, UserMenus);
	}

	// --- Private Saving Logic (Generic) ---

	private static void SaveDataToFile<T>(string filePath, T data)
	{
		try
		{
			// Serialize using the defined options
			string json = JsonSerializer.Serialize(data, SerializerOptions);
			File.WriteAllText(filePath, json);
		}
		catch (Exception ex)
		{
			ErrorLogger.LogError(ex);
			Console.WriteLine($"Error writing {filePath}: {ex.Message}");
		}
	}
}