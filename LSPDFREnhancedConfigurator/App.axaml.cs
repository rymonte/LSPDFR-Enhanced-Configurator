using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using LSPDFREnhancedConfigurator.Services;
using LSPDFREnhancedConfigurator.UI.Views;
using LSPDFREnhancedConfigurator.UI.ViewModels;

namespace LSPDFREnhancedConfigurator
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private static Bitmap LoadBitmapFromResource(string resourcePath)
        {
            var uri = new Uri($"avares://LSPDFREnhancedConfigurator{resourcePath}");
            return new Bitmap(AssetLoader.Open(uri));
        }

        public override async void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Initialize settings manager
                var settingsManager = new SettingsManager();
                Logger.Debug("SettingsManager initialized");

                // Set logger verbosity from settings
                var logVerbosity = settingsManager.GetLogVerbosity();
                Logger.CurrentLogLevel = logVerbosity;
                Logger.Info($"Logger verbosity set to: {logVerbosity}");

                // Check for GTA V directory
                string? gtaRootPath = settingsManager.GetGtaVDirectory();
                bool skipWelcome = settingsManager.GetSkipWelcomeScreen();

                // Show welcome screen if requested or if no valid directory
                var validation = StartupValidationService.ValidateGtaDirectory(gtaRootPath);
                if (!skipWelcome || !validation.IsValid)
                {
                    // Show welcome window
                    var welcomeViewModel = new WelcomeWindowViewModel(settingsManager);
                    var welcomeWindow = new WelcomeWindow
                    {
                        DataContext = welcomeViewModel
                    };

                    desktop.MainWindow = welcomeWindow;

                    // Set up handler for when welcome window closes
                    welcomeWindow.Closed += async (sender, args) =>
                    {
                        // After welcome window closes, reload settings and continue
                        gtaRootPath = settingsManager.GetGtaVDirectory();
                        var newValidation = StartupValidationService.ValidateGtaDirectory(gtaRootPath);

                        if (newValidation.IsValid)
                        {
                            // Continue with normal startup
                            await ContinueStartup(desktop, settingsManager, gtaRootPath);
                        }
                        else
                        {
                            // User cancelled or didn't provide valid directory - shut down
                            Logger.Info("Application closing - no valid GTA directory provided");
                            desktop.Shutdown();
                        }
                    };

                    welcomeWindow.Show();
                    return;
                }

                // Validate GTA V directory one more time before proceeding
                var originalPath = gtaRootPath; // Store original path for reversion
                if (!validation.IsValid)
                {
                    Logger.Error($"GTA V directory validation failed ({validation.Severity}): {validation.ErrorMessage}");

                    // Determine window title and header color based on severity
                    string windowTitle = validation.Severity == GtaValidationSeverity.Error
                        ? "GTA V Installation Error"
                        : "GTA V Installation Warning";

                    IBrush headerColor = validation.Severity == GtaValidationSeverity.Error
                        ? Brushes.Red
                        : Brushes.Orange;

                    // Create directory input controls
                    var directoryTextBox = new TextBox
                    {
                        Watermark = "Enter or paste GTA V directory path...",
                        Text = gtaRootPath ?? "",
                        Margin = new Thickness(0, 5, 0, 0)
                    };

                    var browseButton = new Button
                    {
                        Content = new StackPanel
                        {
                            Orientation = Avalonia.Layout.Orientation.Horizontal,
                            Spacing = 8,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            Children =
                            {
                                new Image
                                {
                                    Source = LoadBitmapFromResource("/Resources/Icons/folder-icon.png"),
                                    Width = 16,
                                    Height = 16,
                                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                                },
                                new TextBlock
                                {
                                    Text = "Browse...",
                                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                                }
                            }
                        },
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 5, 0, 0),
                        MinWidth = 120
                    };

                    var validateButton = new Button
                    {
                        Content = new StackPanel
                        {
                            Orientation = Avalonia.Layout.Orientation.Horizontal,
                            Spacing = 8,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            Children =
                            {
                                new Image
                                {
                                    Source = LoadBitmapFromResource("/Resources/Icons/success-icon.png"),
                                    Width = 16,
                                    Height = 16,
                                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                                },
                                new TextBlock
                                {
                                    Text = "Validate & Continue",
                                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                                }
                            }
                        },
                        HorizontalAlignment = HorizontalAlignment.Center,
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 15, 0, 0),
                        MinWidth = 180
                    };

                    var exitButton = new Button
                    {
                        Content = "Exit",
                        HorizontalAlignment = HorizontalAlignment.Center,
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        MinWidth = 180
                    };

                    // Show error/warning dialog
                    var errorWindow = new Window
                    {
                        Title = windowTitle,
                        Width = 600,
                        Height = 450,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen,
                        CanResize = false,
                        Content = new StackPanel
                        {
                            Margin = new Thickness(20),
                            Spacing = 12,
                            Children =
                            {
                                new StackPanel
                                {
                                    Orientation = Avalonia.Layout.Orientation.Horizontal,
                                    Spacing = 12,
                                    Children =
                                    {
                                        new Image
                                        {
                                            Source = validation.Severity == GtaValidationSeverity.Error
                                                ? LoadBitmapFromResource("/Resources/Icons/error-icon.png")
                                                : LoadBitmapFromResource("/Resources/Icons/warning-icon.png"),
                                            Width = 32,
                                            Height = 32,
                                            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                                        },
                                        new TextBlock
                                        {
                                            Text = validation.Severity == GtaValidationSeverity.Error
                                                ? "Invalid GTA V Installation"
                                                : "Missing Required Components",
                                            FontSize = 18,
                                            FontWeight = FontWeight.Bold,
                                            Foreground = headerColor,
                                            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                                        }
                                    }
                                },
                                new ScrollViewer
                                {
                                    MaxHeight = 140,
                                    Content = new TextBlock
                                    {
                                        Text = validation.ErrorMessage,
                                        TextWrapping = TextWrapping.Wrap
                                    }
                                },
                                new TextBlock
                                {
                                    Text = "Select GTA V Directory:",
                                    FontWeight = FontWeight.SemiBold,
                                    Margin = new Thickness(0, 10, 0, 0)
                                },
                                directoryTextBox,
                                browseButton,
                                validateButton,
                                exitButton
                            }
                        }
                    };

                    // Wire up button handlers

                    // Browse button - opens folder picker
                    browseButton.Click += async (sender, e) =>
                    {
                        var folders = await errorWindow.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
                        {
                            Title = "Select GTA V Installation Directory",
                            AllowMultiple = false
                        });

                        if (folders.Count > 0)
                        {
                            directoryTextBox.Text = folders[0].Path.LocalPath;
                        }
                    };

                    // Validate button - validates and continues with entered path
                    validateButton.Click += async (sender, e) =>
                    {
                        var selectedPath = directoryTextBox.Text?.Trim();

                        if (string.IsNullOrEmpty(selectedPath))
                        {
                            directoryTextBox.Text = "";
                            return;
                        }

                        var newValidation = StartupValidationService.ValidateGtaDirectory(selectedPath);

                        if (newValidation.IsValid)
                        {
                            settingsManager.SetGtaVDirectory(selectedPath);
                            Logger.Info($"GTA V directory set to: {selectedPath}");
                            errorWindow.Close();

                            // Continue with new path
                            gtaRootPath = selectedPath;
                            await ContinueStartup(desktop, settingsManager, gtaRootPath);
                        }
                        else
                        {
                            // Update dialog with new validation error
                            var contentPanel = (StackPanel)errorWindow.Content;
                            var scrollViewer = (ScrollViewer)contentPanel.Children[1];
                            var textBlock = (TextBlock)scrollViewer.Content;
                            textBlock.Text = newValidation.ErrorMessage;

                            // Update header based on new severity
                            var headerPanel = (StackPanel)contentPanel.Children[0];
                            var headerIcon = (Image)headerPanel.Children[0];
                            var headerText = (TextBlock)headerPanel.Children[1];

                            if (newValidation.Severity == GtaValidationSeverity.Error)
                            {
                                headerIcon.Source = LoadBitmapFromResource("/Resources/Icons/error-icon.png");
                                headerText.Text = "Invalid GTA V Installation";
                                headerText.Foreground = Brushes.Red;
                                errorWindow.Title = "GTA V Installation Error";
                            }
                            else
                            {
                                headerIcon.Source = LoadBitmapFromResource("/Resources/Icons/warning-icon.png");
                                headerText.Text = "Missing Required Components";
                                headerText.Foreground = Brushes.Orange;
                                errorWindow.Title = "GTA V Installation Warning";
                            }
                        }
                    };

                    exitButton.Click += (sender, e) =>
                    {
                        // Revert to original path on exit
                        if (!string.IsNullOrEmpty(originalPath))
                        {
                            settingsManager.SetGtaVDirectory(originalPath);
                            Logger.Info($"Reverted GTA V directory to: {originalPath}");
                        }

                        Logger.Info("User chose to exit due to invalid GTA V directory");
                        errorWindow.Close();
                        desktop.Shutdown();
                    };

                    desktop.MainWindow = errorWindow;
                    errorWindow.Show();
                    return;
                }

                await ContinueStartup(desktop, settingsManager, gtaRootPath);
            }

            base.OnFrameworkInitializationCompleted();
        }

        private async Task ContinueStartup(IClassicDesktopStyleApplicationLifetime desktop, SettingsManager settingsManager, string gtaRootPath)
        {
            Logger.Info($"Using GTA V directory: {gtaRootPath}");

            // Get available profiles first
            var profiles = RanksXmlLoader.GetAvailableProfiles(gtaRootPath);
            string? currentProfile = null;

            if (profiles.Count == 0)
            {
                // Create default profile
                try
                {
                    var defaultProfilePath = Path.Combine(gtaRootPath, "plugins", "LSPDFR", "LSPDFR Enhanced", "Profiles", "Default");
                    Directory.CreateDirectory(defaultProfilePath);
                    currentProfile = "Default";
                    Logger.Info("Created default profile");
                }
                catch (Exception ex)
                {
                    Logger.Warn($"Could not create default profile directory: {ex.Message}");
                    currentProfile = "Default"; // Use Default even if we can't create the directory
                }
            }
            else
            {
                // Check if saved profile exists
                var savedProfile = settingsManager.GetSelectedProfile();
                bool savedProfileExists = !string.IsNullOrEmpty(savedProfile) && profiles.Contains(savedProfile);

                // Show profile selection dialog if:
                // 1. Saved profile doesn't exist (was deleted)
                // 2. Or if we want to prompt user on startup
                if (!savedProfileExists)
                {
                    Logger.Debug($"Saved profile '{savedProfile}' not found or invalid. Prompting user to select profile.");

                    // Show profile selection dialog (standalone window)
                    var profileDialogViewModel = new UI.ViewModels.ProfileSelectionDialogViewModel(profiles, settingsManager);
                    var profileDialog = new UI.Views.ProfileSelectionDialog
                    {
                        DataContext = profileDialogViewModel,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen
                    };

                    // Temporarily set dialog as main window to prevent app shutdown
                    desktop.MainWindow = profileDialog;

                    // Create a TaskCompletionSource to wait for dialog result
                    var tcs = new TaskCompletionSource<bool>();
                    profileDialog.Closed += (s, e) => tcs.SetResult(true);

                    profileDialog.Show();
                    await tcs.Task;

                    if (!string.IsNullOrEmpty(profileDialogViewModel.SelectedProfile))
                    {
                        currentProfile = profileDialogViewModel.SelectedProfile;
                        Logger.Info($"User selected profile: {currentProfile}");
                    }
                    else
                    {
                        // User cancelled - shut down application
                        Logger.Info("Profile selection cancelled - shutting down application");
                        desktop.Shutdown();
                        return;
                    }
                }
                else
                {
                    currentProfile = savedProfile;
                    Logger.Info($"Using saved profile: {currentProfile}");
                }
            }

            // Create main window with empty data first, showing loading overlay
            Logger.Debug("Creating main window...");
            var fileDiscovery = new FileDiscoveryService(gtaRootPath);
            var dataService = new DataLoadingService(fileDiscovery);

            var mainViewModel = new MainWindowViewModel(dataService, null, gtaRootPath, currentProfile, settingsManager);
            mainViewModel.IsLoading = true;
            mainViewModel.SetLoadingIndeterminate("Initializing...", "Starting up...");

            var mainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };

            desktop.MainWindow = mainWindow;
            mainWindow.Show();

            try
            {
                // Small delay to ensure window is visible
                await Task.Delay(100);

                // Load Ranks.xml
                Logger.Debug("Starting data load...");
                mainViewModel.UpdateLoadingProgress("Loading ranks...", $"Reading Ranks.xml for profile: {currentProfile}", 20);

                List<Models.RankHierarchy>? loadedRanks = null;
                var ranksPath = RanksXmlLoader.FindRanksXml(gtaRootPath, currentProfile);
                if (ranksPath != null)
                {
                    loadedRanks = RanksXmlLoader.LoadFromFile(ranksPath);
                    Logger.Debug($"Loaded {loadedRanks.Count} ranks from {ranksPath}");
                }

                // Discover data files
                mainViewModel.UpdateLoadingProgress("Discovering data files...", "Scanning for agencies, vehicles, stations, and outfits", 40);

                // Load game data
                mainViewModel.UpdateLoadingProgress("Loading game data...", "Parsing agencies, vehicles, stations, and outfits", 60);
                dataService.LoadAll();

                Logger.Debug($"Loaded data: {dataService.Agencies.Count} agencies, " +
                           $"{dataService.AllVehicles.Count} vehicles, " +
                           $"{dataService.Stations.Count} stations, " +
                           $"{dataService.OutfitVariations.Count} outfits");

                // Link station references for loaded ranks
                if (loadedRanks != null && loadedRanks.Count > 0)
                {
                    mainViewModel.UpdateLoadingProgress("Linking station references...", "Mapping stations to ranks", 70);
                    dataService.LinkStationReferencesForHierarchies(loadedRanks);
                    Logger.Info($"Linked station references for {loadedRanks.Count} rank hierarchies");
                }

                // Initialize ViewModels with loaded data (on UI thread)
                mainViewModel.UpdateLoadingProgress("Initializing UI...", "Setting up tabs and data", 80);
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    mainViewModel.Initialize(dataService, loadedRanks);
                });

                // Run validation if ranks were loaded
                if (loadedRanks != null && loadedRanks.Count > 0)
                {
                    mainViewModel.UpdateLoadingProgress("Validating configuration...", "Checking for issues", 90);
                    var validationService = new StartupValidationService(dataService);
                    var validationResult = validationService.ValidateRanks(loadedRanks);

                    // Update validation in MainWindowViewModel
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        mainViewModel.ValidationErrorCount = validationResult.ErrorCount + validationResult.WarningCount + validationResult.AdvisoryCount;
                        mainViewModel.ValidationErrorsText = validationResult.HasIssues
                            ? validationResult.GetSummary()
                            : "✅ No validation errors found.\n\nAll ranks have valid progression and references.";

                        // Populate ValidationErrorItems for icon visibility
                        mainViewModel.ValidationErrorItems.Clear();
                        foreach (var error in validationResult.Errors)
                        {
                            mainViewModel.ValidationErrorItems.Add(mainViewModel.ParseValidationIssue(error));
                        }
                        foreach (var warning in validationResult.Warnings)
                        {
                            mainViewModel.ValidationErrorItems.Add(mainViewModel.ParseValidationIssue(warning));
                        }
                        foreach (var advisory in validationResult.Advisories)
                        {
                            mainViewModel.ValidationErrorItems.Add(mainViewModel.ParseValidationIssue(advisory));
                        }
                    });

                    // Log all validation results
                    if (validationResult.HasIssues)
                    {
                        Logger.Info($"Validation found {validationResult.ErrorCount} error(s), {validationResult.WarningCount} warning(s), and {validationResult.AdvisoryCount} advisory(ies)");
                    }

                    // Show warning dialog if there are errors or warnings (not for advisories alone)
                    if (validationResult.HasErrors || validationResult.HasWarnings)
                    {

                        await Dispatcher.UIThread.InvokeAsync(async () =>
                        {
                            var warningDialog = new Window
                            {
                                Title = "Configuration Validation",
                                Width = 600,
                                Height = 400,
                                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                                Content = new StackPanel
                                {
                                    Margin = new Thickness(20),
                                    Spacing = 15,
                                    Children =
                                    {
                                        new StackPanel
                                        {
                                            Orientation = Avalonia.Layout.Orientation.Horizontal,
                                            Spacing = 12,
                                            Children =
                                            {
                                                new Image
                                                {
                                                    Source = LoadBitmapFromResource("/Resources/Icons/warning-icon.png"),
                                                    Width = 32,
                                                    Height = 32,
                                                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                                                },
                                                new TextBlock
                                                {
                                                    Text = "Configuration Issues Detected",
                                                    FontSize = 18,
                                                    FontWeight = FontWeight.Bold,
                                                    Foreground = Brushes.Orange,
                                                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                                                }
                                            }
                                        },
                                        new TextBlock
                                        {
                                            Text = $"The loaded Ranks.xml contains {validationResult.ErrorCount} error(s) and {validationResult.WarningCount} warning(s):",
                                            TextWrapping = TextWrapping.Wrap
                                        },
                                        new ScrollViewer
                                        {
                                            MaxHeight = 200,
                                            Content = new TextBlock
                                            {
                                                Text = validationResult.GetSummary(),
                                                TextWrapping = TextWrapping.Wrap,
                                                FontFamily = new Avalonia.Media.FontFamily("Consolas,Courier New,monospace"),
                                                FontSize = 12
                                            }
                                        },
                                        new TextBlock
                                        {
                                            Text = "You can continue using the configurator, or click 'Remove Invalid' to automatically remove all invalid entries.",
                                            TextWrapping = TextWrapping.Wrap,
                                            FontStyle = FontStyle.Italic
                                        },
                                        new StackPanel
                                        {
                                            Orientation = Avalonia.Layout.Orientation.Horizontal,
                                            HorizontalAlignment = HorizontalAlignment.Center,
                                            Spacing = 12,
                                            Margin = new Thickness(0, 15, 0, 0),
                                            Children =
                                            {
                                                new Button
                                                {
                                                    Content = new StackPanel
                                                    {
                                                        Orientation = Avalonia.Layout.Orientation.Horizontal,
                                                        Spacing = 8,
                                                        Children =
                                                        {
                                                            new Image
                                                            {
                                                                Source = LoadBitmapFromResource("/Resources/Icons/delete-icon.png"),
                                                                Width = 16,
                                                                Height = 16,
                                                                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                                                            },
                                                            new TextBlock
                                                            {
                                                                Text = "Remove Invalid",
                                                                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                                                            }
                                                        }
                                                    },
                                                    MinWidth = 140
                                                },
                                                new Button
                                                {
                                                    Content = "OK",
                                                    MinWidth = 120
                                                }
                                            }
                                        }
                                    }
                                }
                            };

                            var buttonPanel = (StackPanel)((StackPanel)warningDialog.Content).Children[4];
                            var removeInvalidButton = (Button)buttonPanel.Children[0];
                            var okButton = (Button)buttonPanel.Children[1];

                            removeInvalidButton.Click += (s, e) =>
                            {
                                Logger.Info("Remove Invalid clicked - removing all invalid entries");

                                // Helper function to remove invalid items from a single rank
                                int RemoveInvalidFromRank(Models.RankHierarchy rank)
                                {
                                    int removed = 0;

                                    // Remove invalid vehicles
                                    var validVehicles = rank.Vehicles
                                        .Where(v => dataService.AllVehicles.Any(gv => gv.Model.Equals(v.Model, StringComparison.OrdinalIgnoreCase)))
                                        .ToList();
                                    removed += rank.Vehicles.Count - validVehicles.Count;
                                    rank.Vehicles.Clear();
                                    foreach (var v in validVehicles) rank.Vehicles.Add(v);

                                    // Remove invalid stations
                                    var validStations = rank.Stations
                                        .Where(s => dataService.Stations.Any(gs => gs.Name.Equals(s.StationName, StringComparison.OrdinalIgnoreCase)))
                                        .ToList();
                                    removed += rank.Stations.Count - validStations.Count;
                                    rank.Stations.Clear();
                                    foreach (var st in validStations) rank.Stations.Add(st);

                                    // Remove invalid outfits
                                    var validOutfits = rank.Outfits
                                        .Where(o => dataService.OutfitVariations.Any(ov => ov.Name.Equals(o, StringComparison.OrdinalIgnoreCase)))
                                        .ToList();
                                    removed += rank.Outfits.Count - validOutfits.Count;
                                    rank.Outfits.Clear();
                                    foreach (var outfit in validOutfits) rank.Outfits.Add(outfit);

                                    return removed;
                                }

                                // Remove all invalid references from ranks and pay bands
                                int totalRemoved = 0;

                                foreach (var rank in loadedRanks)
                                {
                                    // Clean parent rank
                                    totalRemoved += RemoveInvalidFromRank(rank);

                                    // Clean pay bands
                                    if (rank.IsParent && rank.PayBands.Count > 0)
                                    {
                                        foreach (var payBand in rank.PayBands)
                                        {
                                            totalRemoved += RemoveInvalidFromRank(payBand);
                                        }
                                    }
                                }

                                Logger.Debug($"Removed {totalRemoved} invalid entries across all ranks and pay bands");

                                // Reload all ViewModels with cleaned data
                                Dispatcher.UIThread.Post(() =>
                                {
                                    mainViewModel.RanksViewModel.LoadRanks(loadedRanks);
                                    mainViewModel.StationAssignmentsViewModel.LoadRanks(loadedRanks);
                                    mainViewModel.VehiclesViewModel.LoadRanks(loadedRanks);
                                    mainViewModel.OutfitsViewModel.LoadRanks(loadedRanks);
                                    mainViewModel.StatusMessage = $"Removed {totalRemoved} invalid entries";
                                    mainViewModel.ValidationErrorCount = 0;
                                });

                                warningDialog.Close();
                            };

                            okButton.Click += (s, e) => warningDialog.Close();

                            await warningDialog.ShowDialog(mainWindow);
                        });
                    }
                }

                mainViewModel.UpdateLoadingProgress("Complete", "Ready", 100);
                await Task.Delay(300); // Brief pause to show completion

                // Hide loading overlay
                mainViewModel.IsLoading = false;
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to load data", ex);

                // Update loading overlay to show error state (red) on UI thread
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    mainViewModel.LoadingStatusText = "Failed to Load Configuration";
                    mainViewModel.LoadingDetailText = $"Error: {ex.Message}\n\nPlease check the log file for more details or restore from a backup.";
                    mainViewModel.LoadingStatusForeground = "#FF4444"; // Red
                    mainViewModel.LoadingProgressForeground = "#FF4444"; // Red
                    mainViewModel.LoadingProgressBorderBrush = "#FF4444"; // Red
                    mainViewModel.IsLoadingIndeterminate = false;
                    mainViewModel.StatusMessage = $"Error: {ex.Message}";
                });

                // Check for available backups
                var backupFiles = new List<string>();
                try
                {
                    var ranksPath = RanksXmlLoader.FindRanksXml(gtaRootPath, currentProfile);
                    if (ranksPath != null)
                    {
                        var directory = Path.GetDirectoryName(ranksPath);
                        if (directory != null && Directory.Exists(directory))
                        {
                            var fileName = Path.GetFileName(ranksPath);
                            backupFiles = Directory.GetFiles(directory, $"{fileName}.backup_*.xml")
                                .OrderByDescending(f => f)
                                .ToList();
                        }
                    }
                }
                catch (Exception backupEx)
                {
                    Logger.Warn($"Could not scan for backup files: {backupEx.Message}");
                }

                bool hasBackups = backupFiles.Count > 0;
                Logger.Debug($"Found {backupFiles.Count} backup file(s)");

                // Enable error state on loading overlay
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    mainViewModel.HasBackupsAvailable = hasBackups;
                    mainViewModel.HasLoadingError = true;
                    // Keep IsLoading = true so overlay stays visible

                    // Set up retry load action
                    mainViewModel.SetRetryLoadAction(async () =>
                    {
                        Logger.Debug("Retry load action invoked - attempting to reload data");
                        await Dispatcher.UIThread.InvokeAsync(async () =>
                        {
                            // Reset error state
                            mainViewModel.HasLoadingError = false;
                            mainViewModel.LoadingStatusText = "Retrying...";
                            mainViewModel.LoadingDetailText = "Attempting to reload configuration...";
                            mainViewModel.LoadingStatusForeground = "#00D9FF"; // Back to cyan
                            mainViewModel.LoadingProgressForeground = "#00D9FF";
                            mainViewModel.LoadingProgressBorderBrush = "#00D9FF";
                            mainViewModel.LoadingProgress = 0;

                            // Close and reload
                            mainWindow.Close();
                            await ContinueStartup(desktop, settingsManager, gtaRootPath);
                        });
                    });

                    // Set up restore backup action
                    mainViewModel.SetRestoreBackupAction(async () =>
                    {
                        await Dispatcher.UIThread.InvokeAsync(async () =>
                        {
                            try
                            {
                                Logger.Info("Restore from Backup action invoked");

                                // Show restore backup dialog
                                var restoreViewModel = new RestoreBackupDialogViewModel(gtaRootPath, currentProfile, settingsManager);
                                var restoreDialog = new RestoreBackupDialog
                                {
                                    DataContext = restoreViewModel
                                };

                                var restoreResult = await restoreDialog.ShowDialog<bool>(mainWindow);

                                if (restoreResult)
                                {
                                    // Backup was restored successfully - auto-reload for error state
                                    Logger.Info("Backup restored successfully - reloading application");

                                    // Show brief success message
                                    var successDialog = new Window
                                    {
                                        Title = "Backup Restored",
                                        Width = 500,
                                        Height = 250,
                                        WindowStartupLocation = WindowStartupLocation.CenterOwner,
                                        CanResize = false,
                                        Content = new StackPanel
                                        {
                                            Margin = new Thickness(20),
                                            Spacing = 15,
                                            Children =
                                            {
                                                new StackPanel
                                                {
                                                    Orientation = Avalonia.Layout.Orientation.Horizontal,
                                                    Spacing = 12,
                                                    Children =
                                                    {
                                                        new Image
                                                        {
                                                            Source = LoadBitmapFromResource("/Resources/Icons/success-icon.png"),
                                                            Width = 32,
                                                            Height = 32,
                                                            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                                                        },
                                                        new TextBlock
                                                        {
                                                            Text = "Backup Restored Successfully",
                                                            FontSize = 18,
                                                            FontWeight = FontWeight.Bold,
                                                            Foreground = Brushes.LimeGreen,
                                                            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                                                        }
                                                    }
                                                },
                                                new TextBlock
                                                {
                                                    Text = "Your configuration has been restored from the backup file.\n\nThe application will now reload all data.",
                                                    TextWrapping = TextWrapping.Wrap
                                                },
                                                new Button
                                                {
                                                    Content = "OK",
                                                    HorizontalAlignment = HorizontalAlignment.Center,
                                                    MinWidth = 120,
                                                    Margin = new Thickness(0, 15, 0, 0)
                                                }
                                            }
                                        }
                                    };

                                    var successButtonPanel = (StackPanel)successDialog.Content;
                                    var okButton = (Button)successButtonPanel.Children[2];
                                    okButton.Click += async (s2, e2) =>
                                    {
                                        successDialog.Close();

                                        // Reload the application by calling ContinueStartup again
                                        // Close old window AFTER creating new one to prevent app shutdown
                                        Logger.Info("Reloading application after successful backup restoration");
                                        var oldWindow = mainWindow;
                                        await ContinueStartup(desktop, settingsManager, gtaRootPath);
                                        oldWindow.Close();
                                    };

                                    await successDialog.ShowDialog(mainWindow);
                                }
                                else
                                {
                                    // User cancelled restoration
                                    Logger.Info("Backup restoration cancelled by user");
                                }
                            }
                            catch (Exception restoreEx)
                            {
                                Logger.Error($"Error during backup restoration: {restoreEx.Message}");

                                // Show error dialog
                                var restoreErrorDialog = new Window
                                {
                                    Title = "Restore Failed",
                                    Width = 500,
                                    Height = 250,
                                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                                    CanResize = false,
                                    Content = new StackPanel
                                    {
                                        Margin = new Thickness(20),
                                        Spacing = 15,
                                        Children =
                                        {
                                            new StackPanel
                                            {
                                                Orientation = Avalonia.Layout.Orientation.Horizontal,
                                                Spacing = 12,
                                                Children =
                                                {
                                                    new Image
                                                    {
                                                        Source = LoadBitmapFromResource("/Resources/Icons/error-icon.png"),
                                                        Width = 32,
                                                        Height = 32,
                                                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                                                    },
                                                    new TextBlock
                                                    {
                                                        Text = "Backup Restoration Failed",
                                                        FontSize = 18,
                                                        FontWeight = FontWeight.Bold,
                                                        Foreground = Brushes.Red,
                                                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                                                    }
                                                }
                                            },
                                            new TextBlock
                                            {
                                                Text = $"Failed to restore backup:\n\n{restoreEx.Message}",
                                                TextWrapping = TextWrapping.Wrap
                                            },
                                            new Button
                                            {
                                                Content = "OK",
                                                HorizontalAlignment = HorizontalAlignment.Center,
                                                MinWidth = 120,
                                                Margin = new Thickness(0, 15, 0, 0)
                                            }
                                        }
                                    }
                                };

                                var restoreErrorButtonPanel = (StackPanel)restoreErrorDialog.Content;
                                var restoreErrorOkButton = (Button)restoreErrorButtonPanel.Children[2];
                                restoreErrorOkButton.Click += (s2, e2) => restoreErrorDialog.Close();

                                await restoreErrorDialog.ShowDialog(mainWindow);
                            }
                        });
                    });
                });
            }
        }
    }
}
