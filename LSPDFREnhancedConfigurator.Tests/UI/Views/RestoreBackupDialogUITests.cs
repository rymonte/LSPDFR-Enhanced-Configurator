using Avalonia.Headless.XUnit;
using FluentAssertions;
using LSPDFREnhancedConfigurator.Services;
using LSPDFREnhancedConfigurator.UI.ViewModels;
using LSPDFREnhancedConfigurator.UI.Views;
using System.IO;
using Xunit;

namespace LSPDFREnhancedConfigurator.Tests.UI.Views;

public class RestoreBackupDialogUITests
{
    private string CreateTestGtaDirectory()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"test_gta_{System.Guid.NewGuid()}");
        var ranksDir = Path.Combine(tempDir, "plugins", "LSPDFR", "LSPDFR Enhanced", "Profiles", "Default");
        Directory.CreateDirectory(ranksDir);
        return tempDir;
    }

    private SettingsManager CreateTestSettingsManager()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_settings_{System.Guid.NewGuid()}.ini");
        return new SettingsManager(tempPath);
    }

    [AvaloniaFact]
    public void RestoreBackupDialog_Opens_Successfully()
    {
        // Arrange
        var gtaDir = CreateTestGtaDirectory();
        var settingsManager = CreateTestSettingsManager();
        var viewModel = new RestoreBackupDialogViewModel(gtaDir, "Default", settingsManager);

        try
        {
            // Act
            var dialog = new RestoreBackupDialog
            {
                DataContext = viewModel
            };

            // Assert
            dialog.Should().NotBeNull();
            dialog.DataContext.Should().Be(viewModel);
        }
        finally
        {
            if (Directory.Exists(gtaDir))
            {
                Directory.Delete(gtaDir, true);
            }
        }
    }

    [AvaloniaFact]
    public void RestoreBackupDialog_HasCorrectTitle()
    {
        // Arrange
        var gtaDir = CreateTestGtaDirectory();
        var settingsManager = CreateTestSettingsManager();
        var viewModel = new RestoreBackupDialogViewModel(gtaDir, "Default", settingsManager);

        try
        {
            // Act
            var dialog = new RestoreBackupDialog
            {
                DataContext = viewModel
            };

            // Assert
            dialog.Title.Should().Be("Restore from Backup");
        }
        finally
        {
            if (Directory.Exists(gtaDir))
            {
                Directory.Delete(gtaDir, true);
            }
        }
    }

    [AvaloniaFact]
    public void RestoreBackupDialog_WithNoBackups_ShowsNoBackupsMessage()
    {
        // Arrange
        var gtaDir = CreateTestGtaDirectory();
        var settingsManager = CreateTestSettingsManager();
        var viewModel = new RestoreBackupDialogViewModel(gtaDir, "Default", settingsManager);

        try
        {
            // Act
            var dialog = new RestoreBackupDialog
            {
                DataContext = viewModel
            };

            // Assert
            viewModel.HasBackups.Should().BeFalse("no backup files exist");
        }
        finally
        {
            if (Directory.Exists(gtaDir))
            {
                Directory.Delete(gtaDir, true);
            }
        }
    }

    [AvaloniaFact]
    public void RestoreBackupDialog_WithBackups_LoadsBackupList()
    {
        // Arrange
        var gtaDir = CreateTestGtaDirectory();
        var settingsManager = CreateTestSettingsManager();

        // Set backup directory in settings
        var backupDir = Path.Combine(Path.GetTempPath(), $"test_backups_{System.Guid.NewGuid()}", "Default");
        Directory.CreateDirectory(backupDir);
        settingsManager.SetBackupDirectory(Path.GetDirectoryName(backupDir));

        // Create a backup file with proper naming (Ranks_YYYYMMDD_HHMMSS.xml)
        var timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var backupFile = Path.Combine(backupDir, $"Ranks_{timestamp}.xml");
        File.WriteAllText(backupFile, "<Ranks></Ranks>");

        var viewModel = new RestoreBackupDialogViewModel(gtaDir, "Default", settingsManager);

        try
        {
            // Act
            var dialog = new RestoreBackupDialog
            {
                DataContext = viewModel
            };

            // Assert
            viewModel.HasBackups.Should().BeTrue("backup file exists");
        }
        finally
        {
            if (Directory.Exists(gtaDir))
            {
                Directory.Delete(gtaDir, true);
            }
            if (Directory.Exists(backupDir))
            {
                Directory.Delete(backupDir, true);
            }
        }
    }

    [AvaloniaFact]
    public void RestoreBackupDialog_ViewModel_InitializesProperties()
    {
        // Arrange
        var gtaDir = CreateTestGtaDirectory();
        var settingsManager = CreateTestSettingsManager();
        var viewModel = new RestoreBackupDialogViewModel(gtaDir, "Default", settingsManager);

        try
        {
            // Act
            var dialog = new RestoreBackupDialog
            {
                DataContext = viewModel
            };

            // Assert - SelectedBackupPath will be null when no backups exist
            viewModel.SelectedBackupPath.Should().BeNullOrEmpty();
        }
        finally
        {
            if (Directory.Exists(gtaDir))
            {
                Directory.Delete(gtaDir, true);
            }
        }
    }
}
