using System;
using System.IO;
using FluentAssertions;
using LSPDFREnhancedConfigurator.Services;
using LSPDFREnhancedConfigurator.UI.ViewModels;
using Xunit;

namespace LSPDFREnhancedConfigurator.Tests.UI.ViewModels
{
    /// <summary>
    /// Tests for RestoreBackupDialogViewModel
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Component", "ViewModels")]
    public class RestoreBackupDialogViewModelTests : IDisposable
    {
        private readonly string _tempDirectory;
        private readonly string _testProfile = "TestProfile";
        private readonly SettingsManager _settingsManager;

        public RestoreBackupDialogViewModelTests()
        {
            _tempDirectory = Path.Combine(Path.GetTempPath(), $"RestoreBackupTests_{Guid.NewGuid()}");
            Directory.CreateDirectory(_tempDirectory);

            var settingsPath = Path.Combine(_tempDirectory, "test_settings.ini");
            _settingsManager = new SettingsManager(settingsPath);
            _settingsManager.SetGtaVDirectory(_tempDirectory);
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(_tempDirectory))
                {
                    Directory.Delete(_tempDirectory, recursive: true);
                }
            }
            catch { /* Best effort cleanup */ }
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_InitializesSuccessfully()
        {
            // Act
            var viewModel = new RestoreBackupDialogViewModel(_tempDirectory, _testProfile, _settingsManager);

            // Assert
            viewModel.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_InitializesCommands()
        {
            // Act
            var viewModel = new RestoreBackupDialogViewModel(_tempDirectory, _testProfile, _settingsManager);

            // Assert
            viewModel.ChooseBackupCommand.Should().NotBeNull();
            viewModel.ConfirmCommand.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_SetsDefaultInstructionsText()
        {
            // Act
            var viewModel = new RestoreBackupDialogViewModel(_tempDirectory, _testProfile, _settingsManager);

            // Assert - When no backups exist, shows "No backups found" message
            viewModel.InstructionsText.Should().Be("No backups found for this profile.");
        }

        [Fact]
        public void Constructor_WithNoBackups_SetsHasBackupsFalse()
        {
            // Act
            var viewModel = new RestoreBackupDialogViewModel(_tempDirectory, _testProfile, _settingsManager);

            // Assert
            viewModel.HasBackups.Should().BeFalse("no backups exist in temp directory");
        }

        #endregion

        #region Property Tests

        [Fact]
        public void InstructionsText_CanBeSet()
        {
            // Arrange
            var viewModel = new RestoreBackupDialogViewModel(_tempDirectory, _testProfile, _settingsManager);

            // Act
            viewModel.InstructionsText = "Custom instructions";

            // Assert
            viewModel.InstructionsText.Should().Be("Custom instructions");
        }

        [Fact]
        public void FileDetailsText_DefaultsToEmpty()
        {
            // Act
            var viewModel = new RestoreBackupDialogViewModel(_tempDirectory, _testProfile, _settingsManager);

            // Assert - When no backups exist, shows helpful message
            viewModel.FileDetailsText.Should().Contain("No backup files exist");
        }

        [Fact]
        public void FileDetailsText_CanBeSet()
        {
            // Arrange
            var viewModel = new RestoreBackupDialogViewModel(_tempDirectory, _testProfile, _settingsManager);

            // Act
            viewModel.FileDetailsText = "Backup from 2024-01-15";

            // Assert
            viewModel.FileDetailsText.Should().Be("Backup from 2024-01-15");
        }

        [Fact]
        public void ChooseButtonText_DefaultsToChooseBackupFile()
        {
            // Act
            var viewModel = new RestoreBackupDialogViewModel(_tempDirectory, _testProfile, _settingsManager);

            // Assert - When no backups exist, shows manual choice option
            viewModel.ChooseButtonText.Should().Be("Choose File Manually");
        }

        [Fact]
        public void ChooseButtonText_CanBeSet()
        {
            // Arrange
            var viewModel = new RestoreBackupDialogViewModel(_tempDirectory, _testProfile, _settingsManager);

            // Act
            viewModel.ChooseButtonText = "Select File";

            // Assert
            viewModel.ChooseButtonText.Should().Be("Select File");
        }

        [Fact]
        public void ConfirmButtonText_DefaultsToYes()
        {
            // Act
            var viewModel = new RestoreBackupDialogViewModel(_tempDirectory, _testProfile, _settingsManager);

            // Assert - When no backups exist, shows "Close" instead of "Yes"
            viewModel.ConfirmButtonText.Should().Be("Close");
        }

        [Fact]
        public void ConfirmButtonText_CanBeSet()
        {
            // Arrange
            var viewModel = new RestoreBackupDialogViewModel(_tempDirectory, _testProfile, _settingsManager);

            // Act
            viewModel.ConfirmButtonText = "Restore";

            // Assert
            viewModel.ConfirmButtonText.Should().Be("Restore");
        }

        [Fact]
        public void HasBackups_CanBeSet()
        {
            // Arrange
            var viewModel = new RestoreBackupDialogViewModel(_tempDirectory, _testProfile, _settingsManager);

            // Act
            viewModel.HasBackups = true;

            // Assert
            viewModel.HasBackups.Should().BeTrue();
        }

        [Fact]
        public void SelectedBackupPath_DefaultsToNull()
        {
            // Act
            var viewModel = new RestoreBackupDialogViewModel(_tempDirectory, _testProfile, _settingsManager);

            // Assert
            viewModel.SelectedBackupPath.Should().BeNull("no backup selected initially");
        }

        #endregion

        #region Command Tests

        [Fact]
        public void ChooseBackupCommand_IsCreated()
        {
            // Act
            var viewModel = new RestoreBackupDialogViewModel(_tempDirectory, _testProfile, _settingsManager);

            // Assert
            viewModel.ChooseBackupCommand.Should().NotBeNull();
        }

        [Fact]
        public void ConfirmCommand_IsCreated()
        {
            // Act
            var viewModel = new RestoreBackupDialogViewModel(_tempDirectory, _testProfile, _settingsManager);

            // Assert
            viewModel.ConfirmCommand.Should().NotBeNull();
        }

        #endregion
    }
}
