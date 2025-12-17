using System.Collections.ObjectModel;
using System.ComponentModel;
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
	ObservableCollection<Menu> UserMenus { get; set; }
	int Port { get; set; }

	void SavePort();
	void SaveUserMenus();
}

public sealed class LocalData : ILocalData, INotifyPropertyChanged
{
	public event PropertyChangedEventHandler? PropertyChanged;
	private void OnPropertyChanged(string name) =>
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

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
		var menus = LoadDataFromFile<List<Menu>>(MenuPath, new List<Menu>());
		UserMenus = new ObservableCollection<Menu>(menus);

		Port = LoadPort();
	}

	// --- Public Properties ---

	public AvailableCommands AvailableCommands { get; }
	public PluginEnvironment Environment { get; }
	public AvailableMenus AvailableMenus { get; set; }

	private ObservableCollection<Menu> _userMenus;
	public ObservableCollection<Menu> UserMenus
	{
		get => _userMenus;
		set
		{
			if (_userMenus == value) return;
			_userMenus = value;
			OnPropertyChanged(nameof(UserMenus));
		}
	}


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

	public void UpdateUserMenu(Menu menu)
	{
		var existingMenuIndex = UserMenus
			.Select((item, index) => new { item, index })
			.FirstOrDefault(x => x.item.Id == menu.Id)?.index ?? -1;

		if (existingMenuIndex != -1)
		{
			// Replace item: triggers CollectionChanged(Replace)
			UserMenus[existingMenuIndex] = menu;
		}
		else
		{
			// Add item: triggers CollectionChanged(Add)
			UserMenus.Add(menu);
		}
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