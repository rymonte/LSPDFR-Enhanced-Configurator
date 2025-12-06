using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using LSPDFREnhancedConfigurator.Services;

namespace LSPDFREnhancedConfigurator.UI.ViewModels
{
    public enum ValidationFilterLevel
    {
        ShowAll = 0,
        WarningsAndErrorsOnly = 1,
        ErrorsOnly = 2
    }

    public class SettingsViewModel : ViewModelBase
    {
        private readonly SettingsManager _settingsManager;
        private bool _showWelcomeScreen;

        private static Bitmap LoadBitmapFromResource(string resourcePath)
        {
            var uri = new Uri($"avares://LSPDFREnhancedConfigurator{resourcePath}");
            return new Bitmap(AssetLoader.Open(uri));
        }
        private bool _autoValidate = true;
        private bool _showWarnings = true;
        private bool _confirmOverwrites = true;
        private bool _autoSave = false;
        private bool _showAdvancedSettings = false;
        private string _gtaDirectory = string.Empty;
        private string _backupDirectory = string.Empty;
        private string _gtaDirectoryStatus = string.Empty;
        private string _gtaDirectoryStatusColor = "#FF6B6B";

        public event EventHandler? ShowAdvancedSettingsChanged;
        public event EventHandler? GtaDirectoryChanged;
        public event EventHandler? BackupDirectoryChanged;

        public SettingsViewModel(SettingsManager settingsManager)
        {
            _settingsManager = settingsManager;
            ChangeGtaDirectoryCommand = new RelayCommand(async () => await ChangeGtaDirectory());
            ChangeBackupDirectoryCommand = new RelayCommand(async () => await ChangeBackupDirectory());

            // Initialize validation severity levels
            ValidationSeverityLevels = new ObservableCollection<ValidationSeverityOption>
            {
                new ValidationSeverityOption { Level = ValidationFilterLevel.ShowAll, Name = "Show All", Description = "Show errors, warnings, and advisories" },
                new ValidationSeverityOption { Level = ValidationFilterLevel.WarningsAndErrorsOnly, Name = "Warnings and Errors Only", Description = "Show only warnings and errors" },
                new ValidationSeverityOption { Level = ValidationFilterLevel.ErrorsOnly, Name = "Errors Only", Description = "Show only critical errors" }
            };

            LoadSettings();
        }

        public ICommand ChangeGtaDirectoryCommand { get; }
        public ICommand ChangeBackupDirectoryCommand { get; }

        #region Properties

        public string GtaDirectory
        {
            get => _gtaDirectory;
            set => SetProperty(ref _gtaDirectory, value);
        }

        public string BackupDirectory
        {
            get => _backupDirectory;
            set => SetProperty(ref _backupDirectory, value);
        }

        public string GtaDirectoryStatus
        {
            get => _gtaDirectoryStatus;
            set => SetProperty(ref _gtaDirectoryStatus, value);
        }

        public string GtaDirectoryStatusColor
        {
            get => _gtaDirectoryStatusColor;
            set => SetProperty(ref _gtaDirectoryStatusColor, value);
        }

        public bool ShowWelcomeScreen
        {
            get => _showWelcomeScreen;
            set
            {
                if (SetProperty(ref _showWelcomeScreen, value))
                {
                    // Inverted: if "Show welcome screen" is checked, we DON'T skip it
                    _settingsManager.SetSkipWelcomeScreen(!value);
                    Logger.Info($"Show welcome screen: {value} (skip={!value})");
                }
            }
        }

        public bool AutoValidate
        {
            get => _autoValidate;
            set
            {
                if (SetProperty(ref _autoValidate, value))
                {
                    Logger.Info($"Auto-validate: {value}");
                }
            }
        }

        public bool ShowWarnings
        {
            get => _showWarnings;
            set
            {
                if (SetProperty(ref _showWarnings, value))
                {
                    Logger.Info($"Show warnings for XP gaps: {value}");
                }
            }
        }

        public bool ConfirmOverwrites
        {
            get => _confirmOverwrites;
            set
            {
                if (SetProperty(ref _confirmOverwrites, value))
                {
                    Logger.Info($"Confirm before overwrites: {value}");
                }
            }
        }

        public bool AutoSave
        {
            get => _autoSave;
            set
            {
                if (SetProperty(ref _autoSave, value))
                {
                    Logger.Info($"Auto-save every 5 minutes: {value}");
                }
            }
        }

        public bool ShowAdvancedSettings
        {
            get => _showAdvancedSettings;
            set
            {
                if (SetProperty(ref _showAdvancedSettings, value))
                {
                    Logger.Info($"Show advanced settings: {value}");
                    ShowAdvancedSettingsChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public ObservableCollection<ValidationSeverityOption> ValidationSeverityLevels { get; }

        private ValidationSeverityOption _selectedValidationSeverity;
        public ValidationSeverityOption SelectedValidationSeverity
        {
            get => _selectedValidationSeverity;
            set
            {
                if (SetProperty(ref _selectedValidationSeverity, value))
                {
                    _settingsManager.SetValidationSeverity(value.Level);
                    ValidationSeverityChanged?.Invoke(this, EventArgs.Empty);
                    Logger.Info($"Validation severity changed to: {value.Name}");
                }
            }
        }

        public event EventHandler? ValidationSeverityChanged;

        #endregion

        private void LoadSettings()
        {
            // Load GTA V directory
            _gtaDirectory = _settingsManager.GetGtaVDirectory() ?? "Not set";
            OnPropertyChanged(nameof(GtaDirectory));

            // Validate GTA V directory
            ValidateGtaDirectory(_gtaDirectory);

            // Load backup directory
            var customBackupDir = _settingsManager.GetBackupDirectory();
            var defaultBackupDir = _settingsManager.GetDefaultBackupDirectory();

            if (!string.IsNullOrEmpty(customBackupDir))
            {
                _backupDirectory = customBackupDir;
            }
            else if (!string.IsNullOrEmpty(defaultBackupDir))
            {
                _backupDirectory = defaultBackupDir;
            }
            else
            {
                _backupDirectory = "Not set (GTA V directory required)";
            }
            OnPropertyChanged(nameof(BackupDirectory));

            // Load "Show welcome screen" setting - inverted because the setting is "skip welcome"
            bool skipWelcome = _settingsManager.GetSkipWelcomeScreen();
            _showWelcomeScreen = !skipWelcome;
            OnPropertyChanged(nameof(ShowWelcomeScreen));

            // Load validation severity level
            var currentSeverityInt = _settingsManager.GetValidationSeverity();
            var currentSeverity = (ValidationFilterLevel)currentSeverityInt;
            _selectedValidationSeverity = ValidationSeverityLevels.FirstOrDefault(l => l.Level == currentSeverity) ?? ValidationSeverityLevels[0]; // Default to Show All
            OnPropertyChanged(nameof(SelectedValidationSeverity));

            // Advanced settings are hidden by default
            _showAdvancedSettings = false;
        }

        private async System.Threading.Tasks.Task ChangeGtaDirectory()
        {
            // Get the main window
            var mainWindow = Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null;

            if (mainWindow == null)
                return;

            // Open folder picker directly
            var folders = await mainWindow.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select GTA V Installation Directory",
                AllowMultiple = false
            });

            if (folders.Count == 0)
                return;

            var selectedPath = folders[0].Path.LocalPath;

            // Validate the selected directory
            var validation = StartupValidationService.ValidateGtaDirectory(selectedPath);

            if (validation.IsValid)
            {
                _settingsManager.SetGtaVDirectory(selectedPath);
                GtaDirectory = selectedPath;
                ValidateGtaDirectory(selectedPath);
                Logger.Info($"GTA V directory changed to: {selectedPath}");

                // Notify that directory changed - app should reload data
                GtaDirectoryChanged?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                Logger.Warn($"Invalid GTA V directory selected ({validation.Severity}): {validation.ErrorMessage}");
                GtaDirectory = selectedPath;
                ValidateGtaDirectory(selectedPath);
            }
        }

        private async System.Threading.Tasks.Task ChangeBackupDirectory()
        {
            // Get the main window
            var mainWindow = Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null;

            if (mainWindow == null)
                return;

            // Open folder picker directly
            var folders = await mainWindow.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select Backup Directory",
                AllowMultiple = false
            });

            if (folders.Count == 0)
                return;

            var selectedPath = folders[0].Path.LocalPath;

            // Check if directory exists or can be created
            if (!System.IO.Directory.Exists(selectedPath))
            {
                try
                {
                    System.IO.Directory.CreateDirectory(selectedPath);
                }
                catch (System.Exception ex)
                {
                    Logger.Warn($"Failed to create backup directory: {ex.Message}");
                    return;
                }
            }

            // Check write permissions
            try
            {
                var testFile = System.IO.Path.Combine(selectedPath, ".write_test");
                System.IO.File.WriteAllText(testFile, "test");
                System.IO.File.Delete(testFile);
            }
            catch (System.Exception ex)
            {
                Logger.Warn($"Backup directory is not writable: {ex.Message}");
                return;
            }

            _settingsManager.SetBackupDirectory(selectedPath);
            BackupDirectory = selectedPath;
            Logger.Info($"Backup directory changed to: {selectedPath}");

            // Notify that directory changed
            BackupDirectoryChanged?.Invoke(this, EventArgs.Empty);
        }

        private void ValidateGtaDirectory(string path)
        {
            if (path == "Not set")
            {
                GtaDirectoryStatus = "⚠ Not configured";
                GtaDirectoryStatusColor = "#FFB84D";
                return;
            }

            var validation = StartupValidationService.ValidateGtaDirectory(path);

            if (validation.IsValid)
            {
                GtaDirectoryStatus = "✓ Valid";
                GtaDirectoryStatusColor = "#4CAF50";
            }
            else
            {
                GtaDirectoryStatus = $"✗ {validation.ErrorMessage}";
                GtaDirectoryStatusColor = "#FF6B6B";
            }
        }

        #region Nested Classes

        public class ValidationSeverityOption
        {
            public ValidationFilterLevel Level { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
        }

        #endregion
    }
}
