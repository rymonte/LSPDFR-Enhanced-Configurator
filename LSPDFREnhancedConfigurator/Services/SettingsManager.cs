using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LSPDFREnhancedConfigurator.Services
{
    /// <summary>
    /// Manages application settings stored in settings.ini
    /// </summary>
    public class SettingsManager
    {
        private readonly string _settingsFilePath;
        private Dictionary<string, string> _settings;

        public SettingsManager(string? settingsPath = null)
        {
            // Store settings.ini next to the executable
            var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            _settingsFilePath = settingsPath ?? Path.Combine(appDirectory, "settings.ini");
            _settings = new Dictionary<string, string>();

            Load();
        }

        /// <summary>
        /// Load settings from settings.ini file
        /// </summary>
        public void Load()
        {
            _settings.Clear();

            if (!File.Exists(_settingsFilePath))
            {
                // Create default settings file
                CreateDefaultSettings();
                return;
            }

            try
            {
                var lines = File.ReadAllLines(_settingsFilePath);

                foreach (var line in lines)
                {
                    // Skip empty lines and comments
                    if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#") || line.TrimStart().StartsWith(";"))
                        continue;

                    // Parse key=value pairs
                    var parts = line.Split(new[] { '=' }, 2);
                    if (parts.Length == 2)
                    {
                        var key = parts[0].Trim();
                        var value = parts[1].Trim();
                        _settings[key] = value;
                    }
                }
            }
            catch (Exception)
            {
                // If loading fails, create default settings
                CreateDefaultSettings();
            }
        }

        /// <summary>
        /// Save current settings to settings.ini file
        /// </summary>
        public void Save()
        {
            try
            {
                var lines = new List<string>
                {
                    "# LSPDFR Enhanced Configurator Settings",
                    "# This file stores application configuration",
                    "",
                    "# GTA V Installation Directory",
                    $"gtaVdirectory={GetString("gtaVdirectory", "")}",
                    "",
                    "# Backup Directory",
                    $"backupDirectory={GetString("backupDirectory", "")}",
                    "",
                    "# Last Selected Profile",
                    $"selectedProfile={GetString("selectedProfile", "Default")}",
                    "",
                    "# Window Settings",
                    $"windowWidth={GetInt("windowWidth", 1400)}",
                    $"windowHeight={GetInt("windowHeight", 900)}",
                    $"windowMaximized={GetBool("windowMaximized", false)}",
                    "",
                    "# UI Preferences",
                    $"xmlPreviewVisible={GetBool("xmlPreviewVisible", false)}",
                    $"showValidationWarnings={GetBool("showValidationWarnings", true)}",
                    $"skipWelcomeScreen={GetBool("skipWelcomeScreen", false)}",
                    "",
                    "# Advanced Settings",
                    $"createBackups={GetBool("createBackups", true)}",
                    $"maxBackups={GetInt("maxBackups", 10)}",
                    $"autoSaveInterval={GetInt("autoSaveInterval", 0)}", // 0 = disabled
                    "",
                    "# Logging Settings",
                    $"logVerbosity={GetInt("logVerbosity", (int)LogLevel.Info)}", // 0=Trace, 1=Debug, 2=Info, 3=Warn, 4=Error
                    "",
                    "# Validation Settings",
                    $"validationSeverity={GetInt("validationSeverity", 0)}", // 0=ShowAll, 1=WarningsAndErrorsOnly, 2=ErrorsOnly
                };

                File.WriteAllLines(_settingsFilePath, lines);
            }
            catch (Exception)
            {
                // Silently fail - settings not critical
            }
        }

        /// <summary>
        /// Create default settings file
        /// </summary>
        private void CreateDefaultSettings()
        {
            _settings.Clear();

            // Default values
            _settings["gtaVdirectory"] = "";
            _settings["backupDirectory"] = ""; // Empty = use default
            _settings["selectedProfile"] = "Default";
            _settings["windowWidth"] = "1400";
            _settings["windowHeight"] = "900";
            _settings["windowMaximized"] = "false";
            _settings["xmlPreviewVisible"] = "false";
            _settings["showValidationWarnings"] = "true";
            _settings["createBackups"] = "true";
            _settings["maxBackups"] = "10";
            _settings["autoSaveInterval"] = "0";
            _settings["logVerbosity"] = ((int)LogLevel.Info).ToString();
            _settings["validationSeverity"] = "0"; // ShowAll

            Save();
        }

        /// <summary>
        /// Get string setting value
        /// </summary>
        public string GetString(string key, string defaultValue = "")
        {
            return _settings.ContainsKey(key) ? _settings[key] : defaultValue;
        }

        /// <summary>
        /// Set string setting value
        /// </summary>
        public void SetString(string key, string value)
        {
            _settings[key] = value;
        }

        /// <summary>
        /// Get integer setting value
        /// </summary>
        public int GetInt(string key, int defaultValue = 0)
        {
            if (_settings.ContainsKey(key) && int.TryParse(_settings[key], out int result))
                return result;
            return defaultValue;
        }

        /// <summary>
        /// Set integer setting value
        /// </summary>
        public void SetInt(string key, int value)
        {
            _settings[key] = value.ToString();
        }

        /// <summary>
        /// Get boolean setting value
        /// </summary>
        public bool GetBool(string key, bool defaultValue = false)
        {
            if (_settings.ContainsKey(key) && bool.TryParse(_settings[key], out bool result))
                return result;
            return defaultValue;
        }

        /// <summary>
        /// Set boolean setting value
        /// </summary>
        public void SetBool(string key, bool value)
        {
            _settings[key] = value.ToString().ToLower();
        }

        /// <summary>
        /// Check if GTA V directory has been configured
        /// </summary>
        public bool HasGtaVDirectory()
        {
            var directory = GetString("gtaVdirectory");
            return !string.IsNullOrEmpty(directory) && Directory.Exists(directory);
        }

        /// <summary>
        /// Get GTA V directory path
        /// </summary>
        public string? GetGtaVDirectory()
        {
            var directory = GetString("gtaVdirectory");
            return string.IsNullOrEmpty(directory) ? null : directory;
        }

        /// <summary>
        /// Set GTA V directory path
        /// </summary>
        public void SetGtaVDirectory(string path)
        {
            SetString("gtaVdirectory", path);
            Save();
        }

        /// <summary>
        /// Get last selected profile
        /// </summary>
        public string GetSelectedProfile()
        {
            return GetString("selectedProfile", "Default");
        }

        /// <summary>
        /// Set selected profile
        /// </summary>
        public void SetSelectedProfile(string profileName)
        {
            SetString("selectedProfile", profileName);
            Save();
        }

        /// <summary>
        /// Get window size
        /// </summary>
        public (int width, int height, bool maximized) GetWindowSize()
        {
            return (
                GetInt("windowWidth", 1400),
                GetInt("windowHeight", 900),
                GetBool("windowMaximized", false)
            );
        }

        /// <summary>
        /// Set window size
        /// </summary>
        public void SetWindowSize(int width, int height, bool maximized)
        {
            SetInt("windowWidth", width);
            SetInt("windowHeight", height);
            SetBool("windowMaximized", maximized);
            Save();
        }

        /// <summary>
        /// Get XML preview visibility
        /// </summary>
        public bool GetXmlPreviewVisible()
        {
            return GetBool("xmlPreviewVisible", false);
        }

        /// <summary>
        /// Set XML preview visibility
        /// </summary>
        public void SetXmlPreviewVisible(bool visible)
        {
            SetBool("xmlPreviewVisible", visible);
            Save();
        }

        /// <summary>
        /// Get whether to skip the welcome screen
        /// </summary>
        public bool GetSkipWelcomeScreen()
        {
            return GetBool("skipWelcomeScreen", false);
        }

        /// <summary>
        /// Set whether to skip the welcome screen
        /// </summary>
        public void SetSkipWelcomeScreen(bool skip)
        {
            SetBool("skipWelcomeScreen", skip);
            Save();
        }

        /// <summary>
        /// Get log verbosity level
        /// </summary>
        public LogLevel GetLogVerbosity()
        {
            int level = GetInt("logVerbosity", (int)LogLevel.Info);
            if (Enum.IsDefined(typeof(LogLevel), level))
                return (LogLevel)level;
            return LogLevel.Info;
        }

        /// <summary>
        /// Set log verbosity level
        /// </summary>
        public void SetLogVerbosity(LogLevel level)
        {
            SetInt("logVerbosity", (int)level);
            Save();
        }

        /// <summary>
        /// Get validation severity level
        /// </summary>
        public int GetValidationSeverity()
        {
            return GetInt("validationSeverity", 0); // Default to ShowAll
        }

        /// <summary>
        /// Set validation severity level
        /// </summary>
        public void SetValidationSeverity(UI.ViewModels.ValidationFilterLevel level)
        {
            SetInt("validationSeverity", (int)level);
            Save();
        }

        /// <summary>
        /// Get custom backup directory path
        /// </summary>
        public string? GetBackupDirectory()
        {
            var directory = GetString("backupDirectory");
            return string.IsNullOrEmpty(directory) ? null : directory;
        }

        /// <summary>
        /// Set custom backup directory path
        /// </summary>
        public void SetBackupDirectory(string path)
        {
            SetString("backupDirectory", path);
            Save();
        }

        /// <summary>
        /// Get default backup directory based on GTA V installation
        /// </summary>
        public string GetDefaultBackupDirectory()
        {
            var gtaDirectory = GetGtaVDirectory();
            if (string.IsNullOrEmpty(gtaDirectory))
                return string.Empty;

            return Path.Combine(gtaDirectory, "plugins", "LSPDFR", "LSPDFR Enhanced", "Configurator", "Backups");
        }

        /// <summary>
        /// Get effective backup directory (custom if set, otherwise default)
        /// </summary>
        public string GetEffectiveBackupDirectory()
        {
            var customDirectory = GetBackupDirectory();

            // If custom directory is set and exists, use it
            if (!string.IsNullOrEmpty(customDirectory) && Directory.Exists(customDirectory))
                return customDirectory;

            // Otherwise, return default directory
            return GetDefaultBackupDirectory();
        }
    }
}
