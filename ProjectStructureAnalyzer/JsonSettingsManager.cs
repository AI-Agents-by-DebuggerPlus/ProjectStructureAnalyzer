using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProjectStructureAnalyzer
{
    public class AppSettings
    {
        [JsonPropertyName("applicationSettings")]
        public ApplicationSettings ApplicationSettings { get; set; } = new ApplicationSettings();

        [JsonPropertyName("filterSettings")]
        public FilterSettings FilterSettings { get; set; } = new FilterSettings();

        [JsonPropertyName("userInterface")]
        public UserInterfaceSettings UserInterface { get; set; } = new UserInterfaceSettings();
    }

    public class ApplicationSettings
    {
        [JsonPropertyName("lastSelectedPath")]
        public string LastSelectedPath { get; set; } = string.Empty;
    }

    public class FilterSettings
    {
        [JsonPropertyName("folderFilters")]
        public List<string> FolderFilters { get; set; } = new List<string> { "bin", "obj", ".git", ".vs", "node_modules", "packages" };

        [JsonPropertyName("fileFilters")]
        public List<string> FileFilters { get; set; } = new List<string> { ".user", ".suo", ".cache", ".tmp", ".log" };

        [JsonPropertyName("enableFolderFilters")]
        public bool EnableFolderFilters { get; set; } = true;

        [JsonPropertyName("enableFileFilters")]
        public bool EnableFileFilters { get; set; } = true;
    }

    public class UserInterfaceSettings
    {
        [JsonPropertyName("fontFamily")]
        public string FontFamily { get; set; } = "Segoe UI";

        [JsonPropertyName("fontSize")]
        public double FontSize { get; set; } = 13.0;

        [JsonPropertyName("theme")]
        public string Theme { get; set; } = "Light";

        [JsonPropertyName("windowWidth")]
        public double WindowWidth { get; set; } = 800;

        [JsonPropertyName("windowHeight")]
        public double WindowHeight { get; set; } = 600;

        [JsonPropertyName("windowState")]
        public string WindowState { get; set; } = "Normal";
    }

    public class JsonSettingsManager
    {
        private readonly string defaultSettingsPath;
        private readonly string userSettingsPath;
        private readonly string settingsDirectory;
        private AppSettings currentSettings;

        public JsonSettingsManager(string defaultPath = "default.json", string userPath = "app_settings.json")
        {
            settingsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ProjectStructureAnalyzer");
            Directory.CreateDirectory(settingsDirectory); // Ensure the directory exists

            defaultSettingsPath = Path.Combine(settingsDirectory, defaultPath);
            userSettingsPath = Path.Combine(settingsDirectory, userPath);
            currentSettings = new AppSettings();
        }

        public AppSettings Settings => currentSettings;

        public void LoadSettings()
        {
            try
            {
                LoadDefaultSettings(); // Load default.json for filter and UI settings
                LoadUserSettings();    // Override LastSelectedPath with app_settings.json if it exists
                Logger.LogInfo("Settings loaded successfully");
            }
            catch (Exception ex)
            {
                Logger.LogError("Error loading settings", ex);
                SetDefaultSettings();
            }
        }

        public void LoadDefaultSettings()
        {
            if (!File.Exists(defaultSettingsPath))
            {
                CreateDefaultSettingsFile();
            }

            var jsonContent = File.ReadAllText(defaultSettingsPath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };

            currentSettings = JsonSerializer.Deserialize<AppSettings>(jsonContent, options) ?? CreateDefaultSettings();
            Logger.LogInfo($"Default settings loaded from: {defaultSettingsPath}");
        }

        public void LoadUserSettings()
        {
            if (File.Exists(userSettingsPath))
            {
                try
                {
                    var jsonContent = File.ReadAllText(userSettingsPath);
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var userSettings = JsonSerializer.Deserialize<Dictionary<string, ApplicationSettings>>(jsonContent, options);
                    if (userSettings != null && userSettings.TryGetValue("applicationSettings", out var appSettings))
                    {
                        if (!string.IsNullOrEmpty(appSettings.LastSelectedPath))
                            currentSettings.ApplicationSettings.LastSelectedPath = appSettings.LastSelectedPath;
                        Logger.LogInfo($"User settings (LastSelectedPath) loaded from: {userSettingsPath}");
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Error loading user settings from {userSettingsPath}", ex);
                }
            }
        }

        public void SaveUserSettings()
        {
            try
            {
                var userSettings = new
                {
                    applicationSettings = new
                    {
                        lastSelectedPath = currentSettings.ApplicationSettings.LastSelectedPath
                    }
                };
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var jsonContent = JsonSerializer.Serialize(userSettings, options);
                File.WriteAllText(userSettingsPath, jsonContent);
                Logger.LogInfo($"User settings (LastSelectedPath) saved to: {userSettingsPath}");
            }
            catch (Exception ex)
            {
                Logger.LogError("Error saving user settings", ex);
                throw;
            }
        }

        private void CreateDefaultSettingsFile()
        {
            try
            {
                var defaultSettings = CreateDefaultSettings();
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var jsonContent = JsonSerializer.Serialize(defaultSettings, options);
                File.WriteAllText(defaultSettingsPath, jsonContent);
                Logger.LogInfo($"Default settings file created: {defaultSettingsPath}");
            }
            catch (Exception ex)
            {
                Logger.LogError("Error creating default settings file", ex);
            }
        }

        public void SetDefaultSettings()
        {
            currentSettings = CreateDefaultSettings();
        }

        private AppSettings CreateDefaultSettings()
        {
            return new AppSettings
            {
                ApplicationSettings = new ApplicationSettings
                {
                    LastSelectedPath = string.Empty
                },
                FilterSettings = new FilterSettings
                {
                    FolderFilters = new List<string> { "bin", "obj", ".git", ".vs", "node_modules", "packages" },
                    FileFilters = new List<string> { ".user", ".suo", ".cache", ".tmp", ".log" },
                    EnableFolderFilters = true,
                    EnableFileFilters = true
                },
                UserInterface = new UserInterfaceSettings
                {
                    FontFamily = "Segoe UI",
                    FontSize = 13.0,
                    Theme = "Light",
                    WindowWidth = 800,
                    WindowHeight = 600,
                    WindowState = "Normal"
                }
            };
        }

        private void MergeSettings(AppSettings userSettings)
        {
            if (userSettings.ApplicationSettings != null && !string.IsNullOrEmpty(userSettings.ApplicationSettings.LastSelectedPath))
            {
                currentSettings.ApplicationSettings.LastSelectedPath = userSettings.ApplicationSettings.LastSelectedPath;
            }
            // FilterSettings and UserInterfaceSettings are not merged from user settings
        }

        public void UpdateFilterSettings(List<string> folderFilters, List<string> fileFilters)
        {
            currentSettings.FilterSettings.FolderFilters = folderFilters ?? new List<string>();
            currentSettings.FilterSettings.FileFilters = fileFilters ?? new List<string>();
            // Do not save to app_settings.json, as filter settings are not stored there
        }

        public void UpdateInterfaceSettings(string fontFamily, double fontSize)
        {
            if (!string.IsNullOrEmpty(fontFamily))
                currentSettings.UserInterface.FontFamily = fontFamily;
            if (fontSize > 0)
                currentSettings.UserInterface.FontSize = fontSize;
            // Do not save to app_settings.json, as UI settings are not stored there
        }

        public void UpdateLastSelectedPath(string path)
        {
            currentSettings.ApplicationSettings.LastSelectedPath = path ?? string.Empty;
            SaveUserSettings(); // Save only LastSelectedPath to app_settings.json
        }
    }
}