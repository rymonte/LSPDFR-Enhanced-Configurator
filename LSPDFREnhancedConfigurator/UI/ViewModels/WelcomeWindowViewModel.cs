using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using LSPDFREnhancedConfigurator.Services;

namespace LSPDFREnhancedConfigurator.UI.ViewModels
{
    public class WelcomeWindowViewModel : ViewModelBase
    {
        private readonly SettingsManager _settingsManager;
        private string _gtaDirectory = string.Empty;
        private bool _skipWelcome = false;
        private bool _canProceed = false;
        private bool _isFirstTimeSetup = true;
        private string _validationMessage = string.Empty;
        private IBrush _validationForeground = Brushes.Red;
        private bool _showValidationMessage = false;

        public WelcomeWindowViewModel(SettingsManager settingsManager)
        {
            _settingsManager = settingsManager;

            BrowseCommand = new RelayCommand(async () => await BrowseDirectory());
            ProceedCommand = new RelayCommand(OnProceed, () => CanProceed);
            CancelCommand = new RelayCommand(OnCancel);
            OpenWebsiteCommand = new RelayCommand(() => OpenUrl("https://lspdfr-enhanced.com/"));
            OpenPluginCommand = new RelayCommand(() => OpenUrl("https://www.lcpdfr.com/downloads/gta5mods/scripts/47267-lspdfr-enhanced-remastered-massive-update/"));

            CheckExistingSetup();
        }

        // For design-time support
        public WelcomeWindowViewModel() : this(new SettingsManager())
        {
        }

        public ICommand BrowseCommand { get; }
        public ICommand ProceedCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand OpenWebsiteCommand { get; }
        public ICommand OpenPluginCommand { get; }

        public string GtaDirectory
        {
            get => _gtaDirectory;
            set
            {
                if (SetProperty(ref _gtaDirectory, value))
                {
                    ValidateDirectory(value);
                }
            }
        }

        public bool SkipWelcome
        {
            get => _skipWelcome;
            set => SetProperty(ref _skipWelcome, value);
        }

        public bool CanProceed
        {
            get => _canProceed;
            set
            {
                if (SetProperty(ref _canProceed, value))
                {
                    ((RelayCommand)ProceedCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsFirstTimeSetup
        {
            get => _isFirstTimeSetup;
            set => SetProperty(ref _isFirstTimeSetup, value);
        }

        public string ValidationMessage
        {
            get => _validationMessage;
            set => SetProperty(ref _validationMessage, value);
        }

        public IBrush ValidationForeground
        {
            get => _validationForeground;
            set => SetProperty(ref _validationForeground, value);
        }

        public bool ShowValidationMessage
        {
            get => _showValidationMessage;
            set => SetProperty(ref _showValidationMessage, value);
        }

        private void CheckExistingSetup()
        {
            var savedPath = _settingsManager.GetGtaVDirectory();

            if (!string.IsNullOrEmpty(savedPath) && Directory.Exists(savedPath))
            {
                Logger.Info($"Found existing GTA V directory: {savedPath}");
                var validation = StartupValidationService.ValidateGtaDirectory(savedPath);

                if (validation.IsValid)
                {
                    // Valid existing setup
                    _gtaDirectory = savedPath;
                    IsFirstTimeSetup = false;
                    CanProceed = true;
                    Logger.Info("Existing GTA V directory is valid");
                }
                else
                {
                    // Existing setup is no longer valid
                    Logger.Warn($"Existing GTA V directory is no longer valid: {validation.ErrorMessage}");
                    ShowFirstTimeSetup();
                }
            }
            else
            {
                // First time setup
                Logger.Info("No existing GTA V directory found - showing first time setup");
                ShowFirstTimeSetup();
            }
        }

        private void ShowFirstTimeSetup()
        {
            IsFirstTimeSetup = true;
            CanProceed = false;
        }

        private async System.Threading.Tasks.Task BrowseDirectory()
        {
            var window = Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.Windows.Count > 0 ? desktop.Windows[0] : null
                : null;

            if (window == null)
                return;

            var folders = await window.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select GTA V Installation Directory",
                AllowMultiple = false
            });

            if (folders.Count > 0)
            {
                GtaDirectory = folders[0].Path.LocalPath;
            }
        }

        private void ValidateDirectory(string path)
        {
            ShowValidationMessage = false;
            ValidationMessage = string.Empty;
            CanProceed = false;

            if (string.IsNullOrWhiteSpace(path))
                return;

            Logger.Info($"Validating selected path: {path}");

            var validation = StartupValidationService.ValidateGtaDirectory(path);

            if (validation.IsValid)
            {
                // Get profile count for success message
                var profilesDir = Path.Combine(path, "plugins", "LSPDFR", "LSPDFR Enhanced", "Profiles");
                int profileCount = 0;
                if (Directory.Exists(profilesDir))
                {
                    profileCount = Directory.GetDirectories(profilesDir).Length;
                }

                if (validation.Severity == GtaValidationSeverity.Warning)
                {
                    ValidationMessage = $"{validation.ErrorMessage}\n\n✓ {profileCount} profile(s) found";
                    ValidationForeground = Brushes.Orange;
                }
                else
                {
                    ValidationMessage = $"✓ Valid GTA V directory found!\n✓ LSPDFR Enhanced detected\n✓ {profileCount} profile(s) found";
                    ValidationForeground = Brushes.Green;
                }
                ShowValidationMessage = true;
                CanProceed = true;
                Logger.Info($"Valid GTA V directory: {path} ({profileCount} profiles found)");
            }
            else
            {
                ValidationMessage = validation.ErrorMessage;
                ValidationForeground = validation.Severity == GtaValidationSeverity.Error ? Brushes.Red : Brushes.Orange;
                ShowValidationMessage = true;
                CanProceed = validation.Severity == GtaValidationSeverity.Warning;
                Logger.Warn($"Directory validation result ({validation.Severity}): {validation.ErrorMessage}");
            }
        }

        private void OnProceed()
        {
            if (string.IsNullOrEmpty(GtaDirectory))
            {
                Logger.Warn("Proceed clicked with empty directory");
                return;
            }

            // Save the selected path
            _settingsManager.SetGtaVDirectory(GtaDirectory);
            Logger.Info($"Saved GTA V directory to settings: {GtaDirectory}");

            // Save skip welcome screen setting
            _settingsManager.SetSkipWelcomeScreen(SkipWelcome);
            Logger.Info($"Skip welcome screen setting saved: {SkipWelcome}");

            // Close the window - the App will then continue to main window
            if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var welcomeWindow = desktop.MainWindow;
                welcomeWindow?.Close();
            }
        }

        private void OnCancel()
        {
            Logger.Info("User cancelled welcome screen");

            if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Shutdown();
            }
        }

        private void OpenUrl(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
                Logger.Info($"Opened link: {url}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to open link: {url}", ex);
            }
        }
    }
}
