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

        [JsonPropertyName("autoSaveSettings")]
        public bool AutoSaveSettings { get; set; } = true;

        [JsonPropertyName("enableLogging")]
        public bool EnableLogging { get; set; } = true;

        [JsonPropertyName("logLevel")]
        public string LogLevel { get; set; } = "Info";
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
        private AppSettings currentSettings;

        public JsonSettingsManager(string defaultPath = "default.json", string userPath = "user_settings.json")
        {
            defaultSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, defaultPath); // Путь к .exe
            userSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, userPath);     // Путь к .exe
            currentSettings = new AppSettings();
        }

        public AppSettings Settings => currentSettings;

        public void LoadSettings()
        {
            try
            {
                LoadDefaultSettings(); // Всегда загружаем default.json как базовые настройки
                LoadUserSettings();   // Переопределяем только если user_settings.json существует
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

                    var userSettings = JsonSerializer.Deserialize<AppSettings>(jsonContent, options);
                    if (userSettings != null)
                    {
                        MergeSettings(userSettings);
                        Logger.LogInfo($"User settings loaded from: {userSettingsPath}");
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
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var jsonContent = JsonSerializer.Serialize(currentSettings, options);
                File.WriteAllText(userSettingsPath, jsonContent);
                Logger.LogInfo($"User settings saved to: {userSettingsPath}");
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
                    LastSelectedPath = string.Empty,
                    AutoSaveSettings = true,
                    EnableLogging = true,
                    LogLevel = "Info"
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
            if (userSettings.ApplicationSettings != null)
            {
                if (!string.IsNullOrEmpty(userSettings.ApplicationSettings.LastSelectedPath))
                    currentSettings.ApplicationSettings.LastSelectedPath = userSettings.ApplicationSettings.LastSelectedPath;
                currentSettings.ApplicationSettings.AutoSaveSettings = userSettings.ApplicationSettings.AutoSaveSettings;
                currentSettings.ApplicationSettings.EnableLogging = userSettings.ApplicationSettings.EnableLogging;
                if (!string.IsNullOrEmpty(userSettings.ApplicationSettings.LogLevel))
                    currentSettings.ApplicationSettings.LogLevel = userSettings.ApplicationSettings.LogLevel;
            }

            if (userSettings.FilterSettings != null)
            {
                if (userSettings.FilterSettings.FolderFilters?.Count > 0)
                    currentSettings.FilterSettings.FolderFilters = userSettings.FilterSettings.FolderFilters;
                if (userSettings.FilterSettings.FileFilters?.Count > 0)
                    currentSettings.FilterSettings.FileFilters = userSettings.FilterSettings.FileFilters;
                currentSettings.FilterSettings.EnableFolderFilters = userSettings.FilterSettings.EnableFolderFilters;
                currentSettings.FilterSettings.EnableFileFilters = userSettings.FilterSettings.EnableFileFilters;
            }

            if (userSettings.UserInterface != null)
            {
                if (!string.IsNullOrEmpty(userSettings.UserInterface.FontFamily))
                    currentSettings.UserInterface.FontFamily = userSettings.UserInterface.FontFamily;
                if (userSettings.UserInterface.FontSize > 0)
                    currentSettings.UserInterface.FontSize = userSettings.UserInterface.FontSize;
                if (!string.IsNullOrEmpty(userSettings.UserInterface.Theme))
                    currentSettings.UserInterface.Theme = userSettings.UserInterface.Theme;
                if (userSettings.UserInterface.WindowWidth > 0)
                    currentSettings.UserInterface.WindowWidth = userSettings.UserInterface.WindowWidth;
                if (userSettings.UserInterface.WindowHeight > 0)
                    currentSettings.UserInterface.WindowHeight = userSettings.UserInterface.WindowHeight;
                if (!string.IsNullOrEmpty(userSettings.UserInterface.WindowState))
                    currentSettings.UserInterface.WindowState = userSettings.UserInterface.WindowState;
            }
        }

        public void UpdateFilterSettings(List<string> folderFilters, List<string> fileFilters)
        {
            currentSettings.FilterSettings.FolderFilters = folderFilters ?? new List<string>();
            currentSettings.FilterSettings.FileFilters = fileFilters ?? new List<string>();
            if (currentSettings.ApplicationSettings.AutoSaveSettings)
            {
                SaveUserSettings();
            }
        }

        public void UpdateInterfaceSettings(string fontFamily, double fontSize)
        {
            if (!string.IsNullOrEmpty(fontFamily))
                currentSettings.UserInterface.FontFamily = fontFamily;
            if (fontSize > 0)
                currentSettings.UserInterface.FontSize = fontSize;
            if (currentSettings.ApplicationSettings.AutoSaveSettings)
            {
                SaveUserSettings();
            }
        }

        public void UpdateLastSelectedPath(string path)
        {
            currentSettings.ApplicationSettings.LastSelectedPath = path ?? string.Empty;
            if (currentSettings.ApplicationSettings.AutoSaveSettings)
            {
                SaveUserSettings();
            }
        }
    }
}