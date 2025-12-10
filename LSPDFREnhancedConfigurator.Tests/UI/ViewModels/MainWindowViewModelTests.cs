using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using LSPDFREnhancedConfigurator.Models;
using LSPDFREnhancedConfigurator.Services;
using LSPDFREnhancedConfigurator.Tests.Builders;
using LSPDFREnhancedConfigurator.UI.ViewModels;
using Moq;
using Xunit;

namespace LSPDFREnhancedConfigurator.Tests.UI.ViewModels
{
    /// <summary>
    /// Tests for MainWindowViewModel covering initialization, commands, and coordination
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Component", "ViewModels")]
    public class MainWindowViewModelTests : IDisposable
    {
        private readonly string _tempDirectory;
        private readonly string _testProfile = "TestProfile";
        private readonly Mock<DataLoadingService> _mockDataService;
        private readonly SettingsManager _settingsManager;

        public MainWindowViewModelTests()
        {
            _tempDirectory = Path.Combine(Path.GetTempPath(), $"MainWindowTests_{Guid.NewGuid()}");
            Directory.CreateDirectory(_tempDirectory);

            var settingsPath = Path.Combine(_tempDirectory, "test_settings.ini");
            _settingsManager = new SettingsManager(settingsPath);
            _settingsManager.SetGtaVDirectory(_tempDirectory);

            _mockDataService = new MockServiceBuilder()
                .WithDefaultAgencies()
                .WithDefaultStations()
                .BuildMock();
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

        #region Initialization Tests

        [Fact]
        public void Constructor_InitializesAllViewModels()
        {
            // Arrange
            var ranks = new List<RankHierarchy>
            {
                new RankHierarchyBuilder().WithName("Officer").Build()
            };

            // Act
            var viewModel = new MainWindowViewModel(
                _mockDataService.Object,
                ranks,
                _tempDirectory,
                _testProfile,
                _settingsManager);

            // Assert
            viewModel.RanksViewModel.Should().NotBeNull();
            viewModel.StationAssignmentsViewModel.Should().NotBeNull();
            viewModel.VehiclesViewModel.Should().NotBeNull();
            viewModel.OutfitsViewModel.Should().NotBeNull();
            viewModel.SettingsViewModel.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_InitializesAllCommands()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();

            // Act
            var viewModel = new MainWindowViewModel(
                _mockDataService.Object,
                ranks,
                _tempDirectory,
                _testProfile,
                _settingsManager);

            // Assert
            viewModel.GenerateCommand.Should().NotBeNull();
            viewModel.UndoCommand.Should().NotBeNull();
            viewModel.RedoCommand.Should().NotBeNull();
            viewModel.RestoreBackupCommand.Should().NotBeNull();
            viewModel.ExitCommand.Should().NotBeNull();
            viewModel.ShowValidationErrorsCommand.Should().NotBeNull();
            viewModel.ToggleXmlPreviewCommand.Should().NotBeNull();
            viewModel.ToggleXmlPreviewThemeCommand.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_SetsInitialProfile()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();

            // Act
            var viewModel = new MainWindowViewModel(
                _mockDataService.Object,
                ranks,
                _tempDirectory,
                _testProfile,
                _settingsManager);

            // Assert
            viewModel.SelectedProfile.Should().Be(_testProfile);
        }

        [Fact]
        public void Constructor_WithNullRanks_InitializesSuccessfully()
        {
            // Act
            var viewModel = new MainWindowViewModel(
                _mockDataService.Object,
                null,
                _tempDirectory,
                _testProfile,
                _settingsManager);

            // Assert
            viewModel.Should().NotBeNull();
            viewModel.RanksViewModel.Should().NotBeNull();
        }

        #endregion

        #region Property Tests

        [Fact]
        public void CurrentTabIndex_CanBeSet()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();
            var viewModel = new MainWindowViewModel(
                _mockDataService.Object,
                ranks,
                _tempDirectory,
                _testProfile,
                _settingsManager);

            // Act
            viewModel.CurrentTabIndex = 2;

            // Assert
            viewModel.CurrentTabIndex.Should().Be(2);
        }

        [Fact]
        public void StatusMessage_DefaultsToReady()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();

            // Act
            var viewModel = new MainWindowViewModel(
                _mockDataService.Object,
                ranks,
                _tempDirectory,
                _testProfile,
                _settingsManager);

            // Assert
            viewModel.StatusMessage.Should().Be("Ready");
        }

        [Fact]
        public void IsXmlPreviewVisible_DefaultsToTrue()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();

            // Act
            var viewModel = new MainWindowViewModel(
                _mockDataService.Object,
                ranks,
                _tempDirectory,
                _testProfile,
                _settingsManager);

            // Assert
            viewModel.IsXmlPreviewVisible.Should().BeTrue();
        }

        [Fact]
        public void IsValidationPanelVisible_DefaultsToFalse()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();

            // Act
            var viewModel = new MainWindowViewModel(
                _mockDataService.Object,
                ranks,
                _tempDirectory,
                _testProfile,
                _settingsManager);

            // Assert
            viewModel.IsValidationPanelVisible.Should().BeFalse();
        }

        [Fact]
        public void ValidationErrorCount_DefaultsToZero()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();

            // Act
            var viewModel = new MainWindowViewModel(
                _mockDataService.Object,
                ranks,
                _tempDirectory,
                _testProfile,
                _settingsManager);

            // Assert
            viewModel.ValidationErrorCount.Should().Be(0);
        }

        #endregion

        #region Command Initialization Tests

        [Fact]
        public void GenerateCommand_IsCreated()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();

            // Act
            var viewModel = new MainWindowViewModel(
                _mockDataService.Object,
                ranks,
                _tempDirectory,
                _testProfile,
                _settingsManager);

            // Assert
            viewModel.GenerateCommand.Should().NotBeNull();
        }

        [Fact]
        public void UndoCommand_IsCreated()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();

            // Act
            var viewModel = new MainWindowViewModel(
                _mockDataService.Object,
                ranks,
                _tempDirectory,
                _testProfile,
                _settingsManager);

            // Assert
            viewModel.UndoCommand.Should().NotBeNull();
        }

        [Fact]
        public void RedoCommand_IsCreated()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();

            // Act
            var viewModel = new MainWindowViewModel(
                _mockDataService.Object,
                ranks,
                _tempDirectory,
                _testProfile,
                _settingsManager);

            // Assert
            viewModel.RedoCommand.Should().NotBeNull();
        }

        [Fact]
        public void RestoreBackupCommand_IsCreated()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();

            // Act
            var viewModel = new MainWindowViewModel(
                _mockDataService.Object,
                ranks,
                _tempDirectory,
                _testProfile,
                _settingsManager);

            // Assert
            viewModel.RestoreBackupCommand.Should().NotBeNull();
        }

        #endregion

        #region ViewModel Integration Tests

        [Fact]
        public void RanksViewModel_IsInitializedWithRanks()
        {
            // Arrange
            var ranks = new List<RankHierarchy>
            {
                new RankHierarchyBuilder().WithName("Officer").Build(),
                new RankHierarchyBuilder().WithName("Detective").Build()
            };

            // Act
            var viewModel = new MainWindowViewModel(
                _mockDataService.Object,
                ranks,
                _tempDirectory,
                _testProfile,
                _settingsManager);

            // Assert
            viewModel.RanksViewModel.Should().NotBeNull();
            // Note: We can't directly inspect RanksViewModel's internal state without additional public properties
        }

        [Fact]
        public void StationAssignmentsViewModel_IsInitializedWithDataService()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();

            // Act
            var viewModel = new MainWindowViewModel(
                _mockDataService.Object,
                ranks,
                _tempDirectory,
                _testProfile,
                _settingsManager);

            // Assert
            viewModel.StationAssignmentsViewModel.Should().NotBeNull();
        }

        [Fact]
        public void VehiclesViewModel_IsInitializedWithDataServiceAndRanks()
        {
            // Arrange
            var ranks = new List<RankHierarchy>
            {
                new RankHierarchyBuilder().WithName("Officer").Build()
            };

            // Act
            var viewModel = new MainWindowViewModel(
                _mockDataService.Object,
                ranks,
                _tempDirectory,
                _testProfile,
                _settingsManager);

            // Assert
            viewModel.VehiclesViewModel.Should().NotBeNull();
        }

        [Fact]
        public void OutfitsViewModel_IsInitializedWithDataServiceAndRanks()
        {
            // Arrange
            var ranks = new List<RankHierarchy>
            {
                new RankHierarchyBuilder().WithName("Officer").Build()
            };

            // Act
            var viewModel = new MainWindowViewModel(
                _mockDataService.Object,
                ranks,
                _tempDirectory,
                _testProfile,
                _settingsManager);

            // Assert
            viewModel.OutfitsViewModel.Should().NotBeNull();
        }

        [Fact]
        public void SettingsViewModel_IsInitializedWithSettingsManager()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();

            // Act
            var viewModel = new MainWindowViewModel(
                _mockDataService.Object,
                ranks,
                _tempDirectory,
                _testProfile,
                _settingsManager);

            // Assert
            viewModel.SettingsViewModel.Should().NotBeNull();
        }

        #endregion

        #region Profile Management Tests

        [Fact]
        public void AvailableProfiles_IsInitialized()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();

            // Act
            var viewModel = new MainWindowViewModel(
                _mockDataService.Object,
                ranks,
                _tempDirectory,
                _testProfile,
                _settingsManager);

            // Assert
            viewModel.AvailableProfiles.Should().NotBeNull();
        }

        [Fact]
        public void SelectedProfile_CanBeChanged()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();
            var viewModel = new MainWindowViewModel(
                _mockDataService.Object,
                ranks,
                _tempDirectory,
                _testProfile,
                _settingsManager);

            var eventRaised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(viewModel.SelectedProfile))
                    eventRaised = true;
            };

            // Act
            viewModel.SelectedProfile = "NewProfile";

            // Assert
            viewModel.SelectedProfile.Should().Be("NewProfile");
            eventRaised.Should().BeTrue("PropertyChanged should be raised");
        }

        #endregion

        #region XML Preview Tests

        [Fact]
        public void ToggleXmlPreviewCommand_TogglesVisibility()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();
            var viewModel = new MainWindowViewModel(
                _mockDataService.Object,
                ranks,
                _tempDirectory,
                _testProfile,
                _settingsManager);

            var initialVisibility = viewModel.IsXmlPreviewVisible;

            // Act
            viewModel.ToggleXmlPreviewCommand.Execute(null);

            // Assert
            viewModel.IsXmlPreviewVisible.Should().Be(!initialVisibility);
        }

        [Fact]
        public void ToggleXmlPreviewThemeCommand_TogglesTheme()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();
            var viewModel = new MainWindowViewModel(
                _mockDataService.Object,
                ranks,
                _tempDirectory,
                _testProfile,
                _settingsManager);

            var initialTheme = viewModel.IsXmlPreviewDarkMode;

            // Act
            viewModel.ToggleXmlPreviewThemeCommand.Execute(null);

            // Assert
            viewModel.IsXmlPreviewDarkMode.Should().Be(!initialTheme);
        }

        #endregion

        #region Validation Panel Tests

        [Fact]
        public void ShowValidationErrorsCommand_TogglesValidationPanel()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();
            var viewModel = new MainWindowViewModel(
                _mockDataService.Object,
                ranks,
                _tempDirectory,
                _testProfile,
                _settingsManager);

            var initialVisibility = viewModel.IsValidationPanelVisible;

            // Act
            viewModel.ShowValidationErrorsCommand.Execute(null);

            // Assert
            viewModel.IsValidationPanelVisible.Should().Be(!initialVisibility);
        }

        [Fact]
        public void RefreshValidationCommand_IsCreated()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();

            // Act
            var viewModel = new MainWindowViewModel(
                _mockDataService.Object,
                ranks,
                _tempDirectory,
                _testProfile,
                _settingsManager);

            // Assert
            viewModel.RefreshValidationCommand.Should().NotBeNull();
        }

        [Fact]
        public void DismissAllWarningsCommand_IsCreated()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();

            // Act
            var viewModel = new MainWindowViewModel(
                _mockDataService.Object,
                ranks,
                _tempDirectory,
                _testProfile,
                _settingsManager);

            // Assert
            viewModel.DismissAllWarningsCommand.Should().NotBeNull();
        }

        [Fact]
        public void DismissAllAdvisoriesCommand_IsCreated()
        {
            // Arrange
            var ranks = new List<RankHierarchy>();

            // Act
            var viewModel = new MainWindowViewModel(
                _mockDataService.Object,
                ranks,
                _tempDirectory,
                _testProfile,
                _settingsManager);

            // Assert
            viewModel.DismissAllAdvisoriesCommand.Should().NotBeNull();
        }

        #endregion
    }
}
