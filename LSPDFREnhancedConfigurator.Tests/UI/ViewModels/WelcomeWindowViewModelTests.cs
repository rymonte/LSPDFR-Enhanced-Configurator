using System;
using System.IO;
using FluentAssertions;
using LSPDFREnhancedConfigurator.Services;
using LSPDFREnhancedConfigurator.UI.ViewModels;
using Xunit;

namespace LSPDFREnhancedConfigurator.Tests.UI.ViewModels
{
    /// <summary>
    /// Tests for WelcomeWindowViewModel covering initialization, validation, and command behavior
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("ViewModel", "WelcomeWindow")]
    public class WelcomeWindowViewModelTests : IDisposable
    {
        private readonly string _tempSettingsFile;

        public WelcomeWindowViewModelTests()
        {
            // Create a temporary settings file for each test
            _tempSettingsFile = Path.Combine(Path.GetTempPath(), $"test_settings_{Guid.NewGuid()}.ini");
        }

        public void Dispose()
        {
            // Clean up temp file
            if (File.Exists(_tempSettingsFile))
            {
                try
                {
                    File.Delete(_tempSettingsFile);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }

        #region Setup Helpers

        private SettingsManager CreateSettingsManager(
            string? savedGtaDirectory = null,
            bool skipWelcome = false)
        {
            var settings = new SettingsManager(_tempSettingsFile);

            if (!string.IsNullOrEmpty(savedGtaDirectory))
            {
                settings.SetGtaVDirectory(savedGtaDirectory);
            }

            settings.SetSkipWelcomeScreen(skipWelcome);
            settings.Save();

            return settings;
        }

        #endregion

        #region Initialization Tests

        [Fact]
        public void Constructor_WithValidSettingsManager_InitializesSuccessfully()
        {
            // Arrange
            var settings = CreateSettingsManager();

            // Act
            var viewModel = new WelcomeWindowViewModel(settings);

            // Assert
            viewModel.Should().NotBeNull();
            viewModel.BrowseCommand.Should().NotBeNull();
            viewModel.ProceedCommand.Should().NotBeNull();
            viewModel.CancelCommand.Should().NotBeNull();
            viewModel.OpenWebsiteCommand.Should().NotBeNull();
            viewModel.OpenPluginCommand.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_WithNoSavedDirectory_ShowsFirstTimeSetup()
        {
            // Arrange
            var settings = CreateSettingsManager(savedGtaDirectory: null);

            // Act
            var viewModel = new WelcomeWindowViewModel(settings);

            // Assert
            viewModel.IsFirstTimeSetup.Should().BeTrue("no existing directory should trigger first-time setup");
            viewModel.CanProceed.Should().BeFalse("should not be able to proceed without valid directory");
            viewModel.GtaDirectory.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_WithInvalidSavedDirectory_ShowsFirstTimeSetup()
        {
            // Arrange
            var settings = CreateSettingsManager(savedGtaDirectory: @"C:\InvalidPath\NonExistent");

            // Act
            var viewModel = new WelcomeWindowViewModel(settings);

            // Assert
            viewModel.IsFirstTimeSetup.Should().BeTrue("invalid saved directory should trigger first-time setup");
            viewModel.CanProceed.Should().BeFalse();
        }

        #endregion

        #region Property Change Tests

        [Fact]
        public void GtaDirectory_WhenSet_RaisesPropertyChanged()
        {
            // Arrange
            var settings = CreateSettingsManager();
            var viewModel = new WelcomeWindowViewModel(settings);
            var propertyChangedRaised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(viewModel.GtaDirectory))
                    propertyChangedRaised = true;
            };

            // Act
            viewModel.GtaDirectory = @"C:\Test\Path";

            // Assert
            propertyChangedRaised.Should().BeTrue();
            viewModel.GtaDirectory.Should().Be(@"C:\Test\Path");
        }

        [Fact]
        public void SkipWelcome_WhenSet_RaisesPropertyChanged()
        {
            // Arrange
            var settings = CreateSettingsManager();
            var viewModel = new WelcomeWindowViewModel(settings);
            var propertyChangedRaised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(viewModel.SkipWelcome))
                    propertyChangedRaised = true;
            };

            // Act
            viewModel.SkipWelcome = true;

            // Assert
            propertyChangedRaised.Should().BeTrue();
            viewModel.SkipWelcome.Should().BeTrue();
        }

        [Fact]
        public void CanProceed_WhenChanged_RaisesCanExecuteOnProceedCommand()
        {
            // Arrange
            var settings = CreateSettingsManager();
            var viewModel = new WelcomeWindowViewModel(settings);
            var canProceedValue = viewModel.ProceedCommand.CanExecute(null);

            // Act
            viewModel.CanProceed = true;

            // Assert
            viewModel.ProceedCommand.CanExecute(null).Should().BeTrue("CanExecute should reflect CanProceed property");
        }

        #endregion

        #region Directory Validation Tests

        [Fact]
        public void GtaDirectory_WhenSetToNull_ClearsValidation()
        {
            // Arrange
            var settings = CreateSettingsManager();
            var viewModel = new WelcomeWindowViewModel(settings);

            // Act
            viewModel.GtaDirectory = null;

            // Assert
            viewModel.ShowValidationMessage.Should().BeFalse();
            viewModel.ValidationMessage.Should().BeEmpty();
            viewModel.CanProceed.Should().BeFalse();
        }

        [Fact]
        public void GtaDirectory_WhenSetToEmpty_ClearsValidation()
        {
            // Arrange
            var settings = CreateSettingsManager();
            var viewModel = new WelcomeWindowViewModel(settings);

            // Act
            viewModel.GtaDirectory = string.Empty;

            // Assert
            viewModel.ShowValidationMessage.Should().BeFalse();
            viewModel.ValidationMessage.Should().BeEmpty();
            viewModel.CanProceed.Should().BeFalse();
        }

        [Fact]
        public void GtaDirectory_WhenSetToWhitespace_ClearsValidation()
        {
            // Arrange
            var settings = CreateSettingsManager();
            var viewModel = new WelcomeWindowViewModel(settings);

            // Act
            viewModel.GtaDirectory = "   ";

            // Assert
            viewModel.ShowValidationMessage.Should().BeFalse();
            viewModel.ValidationMessage.Should().BeEmpty();
            viewModel.CanProceed.Should().BeFalse();
        }

        [Fact]
        public void GtaDirectory_WhenSetToInvalidPath_ShowsErrorMessage()
        {
            // Arrange
            var settings = CreateSettingsManager();
            var viewModel = new WelcomeWindowViewModel(settings);

            // Act
            viewModel.GtaDirectory = @"C:\InvalidPath\NonExistent";

            // Assert
            viewModel.ShowValidationMessage.Should().BeTrue("validation message should be shown for invalid path");
            viewModel.ValidationMessage.Should().NotBeEmpty();
            viewModel.CanProceed.Should().BeFalse("should not be able to proceed with invalid directory");
        }

        #endregion

        #region Command Tests

        [Fact]
        public void ProceedCommand_CanExecute_ReturnsFalseWhenCanProceedIsFalse()
        {
            // Arrange
            var settings = CreateSettingsManager();
            var viewModel = new WelcomeWindowViewModel(settings);
            viewModel.CanProceed = false;

            // Act
            var canExecute = viewModel.ProceedCommand.CanExecute(null);

            // Assert
            canExecute.Should().BeFalse();
        }

        [Fact]
        public void ProceedCommand_CanExecute_ReturnsTrueWhenCanProceedIsTrue()
        {
            // Arrange
            var settings = CreateSettingsManager();
            var viewModel = new WelcomeWindowViewModel(settings);
            viewModel.CanProceed = true;

            // Act
            var canExecute = viewModel.ProceedCommand.CanExecute(null);

            // Assert
            canExecute.Should().BeTrue();
        }

        [Fact]
        public void CancelCommand_CanAlwaysExecute()
        {
            // Arrange
            var settings = CreateSettingsManager();
            var viewModel = new WelcomeWindowViewModel(settings);

            // Act
            var canExecute = viewModel.CancelCommand.CanExecute(null);

            // Assert
            canExecute.Should().BeTrue("Cancel command should always be executable");
        }

        [Fact]
        public void OpenWebsiteCommand_CanAlwaysExecute()
        {
            // Arrange
            var settings = CreateSettingsManager();
            var viewModel = new WelcomeWindowViewModel(settings);

            // Act
            var canExecute = viewModel.OpenWebsiteCommand.CanExecute(null);

            // Assert
            canExecute.Should().BeTrue();
        }

        [Fact]
        public void OpenPluginCommand_CanAlwaysExecute()
        {
            // Arrange
            var settings = CreateSettingsManager();
            var viewModel = new WelcomeWindowViewModel(settings);

            // Act
            var canExecute = viewModel.OpenPluginCommand.CanExecute(null);

            // Assert
            canExecute.Should().BeTrue();
        }

        #endregion
    }
}
