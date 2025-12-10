using System.Collections.Generic;
using FluentAssertions;
using LSPDFREnhancedConfigurator.Models;
using LSPDFREnhancedConfigurator.Services;
using LSPDFREnhancedConfigurator.Tests.Builders;
using LSPDFREnhancedConfigurator.UI.ViewModels;
using Xunit;

namespace LSPDFREnhancedConfigurator.Tests.UI.ViewModels
{
    /// <summary>
    /// Tests for MainWindowViewModel command execution - Undo/Redo delegation, Generate CanExecute, and UI toggles
    /// Note: Many MainWindowViewModel commands require Avalonia UI dialogs and file I/O, which cannot be unit tested.
    /// This test class focuses on commands with testable logic.
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Component", "ViewModels")]
    public class MainWindowViewModelCommandTests
    {
        #region GenerateCommand Tests

        [Fact]
        public void GenerateCommand_CanExecute_WithIsLoadingTrue_ReturnsFalse()
        {
            // Arrange
            var mockService = new MockServiceBuilder().BuildMock();
            var ranks = new List<RankHierarchy> { new RankHierarchyBuilder().WithName("Officer").Build() };
            var settingsManager = CreateTestSettingsManager();
            var viewModel = new MainWindowViewModel(mockService.Object, ranks, "C:\\GTA V", "Default", settingsManager);

            viewModel.IsLoading = true;

            // Act & Assert
            viewModel.GenerateCommand.CanExecute(null).Should().BeFalse("cannot generate while loading");
        }

        [Fact]
        public void GenerateCommand_CanExecute_WithIsLoadingFalse_ReturnsTrue()
        {
            // Arrange
            var mockService = new MockServiceBuilder().BuildMock();
            var ranks = new List<RankHierarchy> { new RankHierarchyBuilder().WithName("Officer").Build() };
            var settingsManager = CreateTestSettingsManager();
            var viewModel = new MainWindowViewModel(mockService.Object, ranks, "C:\\GTA V", "Default", settingsManager);

            viewModel.IsLoading = false;

            // Act & Assert
            viewModel.GenerateCommand.CanExecute(null).Should().BeTrue("can generate when not loading");
        }

        // Note: GenerateCommand.Execute() cannot be unit tested as it requires:
        // 1. Avalonia dialogs (ShowDialog)
        // 2. File I/O (File.WriteAllText, File.Copy)
        // 3. Async execution
        // This is covered by integration/UI tests instead.

        #endregion

        #region Undo/Redo Command Tests

        [Fact]
        public void UndoCommand_CanExecute_WithNoHistory_ReturnsFalse()
        {
            // Arrange
            var mockService = new MockServiceBuilder().BuildMock();
            var ranks = new List<RankHierarchy> { new RankHierarchyBuilder().WithName("Officer").Build() };
            var settingsManager = CreateTestSettingsManager();
            var viewModel = new MainWindowViewModel(mockService.Object, ranks, "C:\\GTA V", "Default", settingsManager);

            // Act & Assert
            viewModel.UndoCommand.CanExecute(null).Should().BeFalse("no undo history");
        }

        // Note: Testing UndoCommand.CanExecute with child ViewModel history is complex due to initialization state.
        // The delegation logic is verified through the Execute tests below.

        [Fact]
        public void UndoCommand_Execute_DelegatesToRanksViewModel()
        {
            // Arrange
            var mockService = new MockServiceBuilder().BuildMock();
            var ranks = new List<RankHierarchy>();
            var settingsManager = CreateTestSettingsManager();
            var viewModel = new MainWindowViewModel(mockService.Object, ranks, "C:\\GTA V", "Default", settingsManager);

            // Add a rank (creates undo history)
            viewModel.RanksViewModel.AddRankCommand.Execute(null);
            viewModel.RanksViewModel.RankTreeItems.Should().HaveCount(1);

            // Act - Undo
            viewModel.UndoCommand.Execute(null);

            // Assert
            viewModel.RanksViewModel.RankTreeItems.Should().BeEmpty("undo should remove the added rank");
        }

        [Fact]
        public void UndoCommand_Execute_WhenStationAssignmentsModified_DelegatesToStationAssignments()
        {
            // Arrange
            var mockService = new MockServiceBuilder().BuildMock();
            var officer = new RankHierarchyBuilder().WithName("Officer").Build();
            officer.Stations.Add(new StationAssignment("Mission Row", new List<string>(), 1));
            var ranks = new List<RankHierarchy> { officer };
            var settingsManager = CreateTestSettingsManager();
            var viewModel = new MainWindowViewModel(mockService.Object, ranks, "C:\\GTA V", "Default", settingsManager);

            // Modify station assignments (creates undo history in StationAssignmentsViewModel)
            viewModel.StationAssignmentsViewModel.RemoveAllStationsCommand.Execute(null);
            officer.Stations.Should().BeEmpty();

            // Act - Undo
            viewModel.UndoCommand.Execute(null);

            // Assert
            officer.Stations.Should().HaveCount(1, "undo should restore the station");
        }

        [Fact]
        public void UndoCommand_Execute_WhenVehiclesModified_DelegatesToVehicles()
        {
            // Arrange
            var mockService = new MockServiceBuilder().BuildMock();
            var officer = new RankHierarchyBuilder().WithName("Officer").Build();
            officer.Vehicles.Add(new Vehicle("police", "Police Cruiser", "LSPD"));
            var ranks = new List<RankHierarchy> { officer };
            var settingsManager = CreateTestSettingsManager();
            var viewModel = new MainWindowViewModel(mockService.Object, ranks, "C:\\GTA V", "Default", settingsManager);

            // Modify vehicles (creates undo history in VehiclesViewModel)
            viewModel.VehiclesViewModel.RemoveAllVehiclesCommand.Execute(null);
            officer.Vehicles.Should().BeEmpty();

            // Act - Undo
            viewModel.UndoCommand.Execute(null);

            // Assert
            officer.Vehicles.Should().HaveCount(1, "undo should restore the vehicle");
        }

        [Fact]
        public void UndoCommand_Execute_WhenOutfitsModified_DelegatesToOutfits()
        {
            // Arrange
            var mockService = new MockServiceBuilder().BuildMock();
            var officer = new RankHierarchyBuilder().WithName("Officer").Build();
            officer.Outfits.Add(OutfitVariationBuilder.CreateMaleVariation().CombinedName);
            var ranks = new List<RankHierarchy> { officer };
            var settingsManager = CreateTestSettingsManager();
            var viewModel = new MainWindowViewModel(mockService.Object, ranks, "C:\\GTA V", "Default", settingsManager);

            // Modify outfits (creates undo history in OutfitsViewModel)
            viewModel.OutfitsViewModel.RemoveAllOutfitsCommand.Execute(null);
            officer.Outfits.Should().BeEmpty();

            // Act - Undo
            viewModel.UndoCommand.Execute(null);

            // Assert
            officer.Outfits.Should().HaveCount(1, "undo should restore the outfit");
        }

        [Fact]
        public void RedoCommand_CanExecute_WithNoRedoHistory_ReturnsFalse()
        {
            // Arrange
            var mockService = new MockServiceBuilder().BuildMock();
            var ranks = new List<RankHierarchy> { new RankHierarchyBuilder().WithName("Officer").Build() };
            var settingsManager = CreateTestSettingsManager();
            var viewModel = new MainWindowViewModel(mockService.Object, ranks, "C:\\GTA V", "Default", settingsManager);

            // Act & Assert
            viewModel.RedoCommand.CanExecute(null).Should().BeFalse("no redo history");
        }

        [Fact]
        public void RedoCommand_CanExecute_AfterUndo_ReturnsTrue()
        {
            // Arrange
            var mockService = new MockServiceBuilder().BuildMock();
            var ranks = new List<RankHierarchy>();
            var settingsManager = CreateTestSettingsManager();
            var viewModel = new MainWindowViewModel(mockService.Object, ranks, "C:\\GTA V", "Default", settingsManager);

            // Add a rank and then undo
            viewModel.RanksViewModel.AddRankCommand.Execute(null);
            viewModel.UndoCommand.Execute(null);

            // Act & Assert
            viewModel.RedoCommand.CanExecute(null).Should().BeTrue("redo available after undo");
        }

        [Fact]
        public void RedoCommand_Execute_RedoesRanksViewModelAction()
        {
            // Arrange
            var mockService = new MockServiceBuilder().BuildMock();
            var ranks = new List<RankHierarchy>();
            var settingsManager = CreateTestSettingsManager();
            var viewModel = new MainWindowViewModel(mockService.Object, ranks, "C:\\GTA V", "Default", settingsManager);

            // Add rank, then undo
            viewModel.RanksViewModel.AddRankCommand.Execute(null);
            viewModel.UndoCommand.Execute(null);
            viewModel.RanksViewModel.RankTreeItems.Should().BeEmpty();

            // Act - Redo
            viewModel.RedoCommand.Execute(null);

            // Assert
            viewModel.RanksViewModel.RankTreeItems.Should().HaveCount(1, "redo should restore the added rank");
        }

        #endregion

        #region ToggleXmlPreviewCommand Tests

        [Fact]
        public void ToggleXmlPreviewCommand_Execute_TogglesIsXmlPreviewVisible()
        {
            // Arrange
            var mockService = new MockServiceBuilder().BuildMock();
            var ranks = new List<RankHierarchy> { new RankHierarchyBuilder().WithName("Officer").Build() };
            var settingsManager = CreateTestSettingsManager();
            var viewModel = new MainWindowViewModel(mockService.Object, ranks, "C:\\GTA V", "Default", settingsManager);

            var initialValue = viewModel.IsXmlPreviewVisible;

            // Act
            viewModel.ToggleXmlPreviewCommand.Execute(null);

            // Assert
            viewModel.IsXmlPreviewVisible.Should().Be(!initialValue, "value should be toggled");
        }

        [Fact]
        public void IsXmlPreviewVisible_Set_UpdatesButtonText()
        {
            // Arrange
            var mockService = new MockServiceBuilder().BuildMock();
            var ranks = new List<RankHierarchy> { new RankHierarchyBuilder().WithName("Officer").Build() };
            var settingsManager = CreateTestSettingsManager();
            var viewModel = new MainWindowViewModel(mockService.Object, ranks, "C:\\GTA V", "Default", settingsManager);

            // Act - Set to false
            viewModel.IsXmlPreviewVisible = false;

            // Assert
            viewModel.XmlPreviewButtonText.Should().Be("Show XML Preview");

            // Act - Set to true
            viewModel.IsXmlPreviewVisible = true;

            // Assert
            viewModel.XmlPreviewButtonText.Should().Be("Hide XML Preview");
        }

        #endregion

        #region ToggleXmlPreviewThemeCommand Tests

        [Fact]
        public void ToggleXmlPreviewThemeCommand_Execute_TogglesIsXmlPreviewDarkMode()
        {
            // Arrange
            var mockService = new MockServiceBuilder().BuildMock();
            var ranks = new List<RankHierarchy> { new RankHierarchyBuilder().WithName("Officer").Build() };
            var settingsManager = CreateTestSettingsManager();
            var viewModel = new MainWindowViewModel(mockService.Object, ranks, "C:\\GTA V", "Default", settingsManager);

            var initialValue = viewModel.IsXmlPreviewDarkMode;

            // Act
            viewModel.ToggleXmlPreviewThemeCommand.Execute(null);

            // Assert
            viewModel.IsXmlPreviewDarkMode.Should().Be(!initialValue, "dark mode should be toggled");
        }

        [Fact]
        public void IsXmlPreviewDarkMode_Set_UpdatesThemeButtonText()
        {
            // Arrange
            var mockService = new MockServiceBuilder().BuildMock();
            var ranks = new List<RankHierarchy> { new RankHierarchyBuilder().WithName("Officer").Build() };
            var settingsManager = CreateTestSettingsManager();
            var viewModel = new MainWindowViewModel(mockService.Object, ranks, "C:\\GTA V", "Default", settingsManager);

            // Act - Set to true (dark mode)
            viewModel.IsXmlPreviewDarkMode = true;

            // Assert
            viewModel.XmlPreviewThemeButtonText.Should().Be("Light Mode");

            // Act - Set to false (light mode)
            viewModel.IsXmlPreviewDarkMode = false;

            // Assert
            viewModel.XmlPreviewThemeButtonText.Should().Be("Dark Mode");
        }

        [Fact]
        public void IsXmlPreviewDarkMode_Set_UpdatesBackgroundAndForeground()
        {
            // Arrange
            var mockService = new MockServiceBuilder().BuildMock();
            var ranks = new List<RankHierarchy> { new RankHierarchyBuilder().WithName("Officer").Build() };
            var settingsManager = CreateTestSettingsManager();
            var viewModel = new MainWindowViewModel(mockService.Object, ranks, "C:\\GTA V", "Default", settingsManager);

            // Act - Dark mode
            viewModel.IsXmlPreviewDarkMode = true;

            // Assert
            viewModel.XmlPreviewBackground.Should().Be("#001928");
            viewModel.XmlPreviewForeground.Should().Be("#E9EAEA");

            // Act - Light mode
            viewModel.IsXmlPreviewDarkMode = false;

            // Assert
            viewModel.XmlPreviewBackground.Should().Be("#FFFFFF");
            viewModel.XmlPreviewForeground.Should().Be("#1E1E1E");
        }

        #endregion

        #region CurrentTabIndex Tests

        [Fact]
        public void CurrentTabIndex_Set_UpdatesValue()
        {
            // Arrange
            var mockService = new MockServiceBuilder().BuildMock();
            var ranks = new List<RankHierarchy> { new RankHierarchyBuilder().WithName("Officer").Build() };
            var settingsManager = CreateTestSettingsManager();
            var viewModel = new MainWindowViewModel(mockService.Object, ranks, "C:\\GTA V", "Default", settingsManager);

            // Act
            viewModel.CurrentTabIndex = 2; // Vehicles tab

            // Assert
            viewModel.CurrentTabIndex.Should().Be(2);
        }

        #endregion

        #region IsLoading Tests

        [Fact]
        public void IsLoading_Set_RaisesGenerateCommandCanExecuteChanged()
        {
            // Arrange
            var mockService = new MockServiceBuilder().BuildMock();
            var ranks = new List<RankHierarchy> { new RankHierarchyBuilder().WithName("Officer").Build() };
            var settingsManager = CreateTestSettingsManager();
            var viewModel = new MainWindowViewModel(mockService.Object, ranks, "C:\\GTA V", "Default", settingsManager);

            // Initially GenerateCommand can execute
            viewModel.IsLoading = false;
            viewModel.GenerateCommand.CanExecute(null).Should().BeTrue();

            // Act - Set IsLoading to true
            viewModel.IsLoading = true;

            // Assert
            viewModel.GenerateCommand.CanExecute(null).Should().BeFalse("Generate disabled when loading");

            // Act - Set IsLoading back to false
            viewModel.IsLoading = false;

            // Assert
            viewModel.GenerateCommand.CanExecute(null).Should().BeTrue("Generate enabled when not loading");
        }

        #endregion

        #region StatusMessage Tests

        [Fact]
        public void StatusMessage_Set_UpdatesValue()
        {
            // Arrange
            var mockService = new MockServiceBuilder().BuildMock();
            var ranks = new List<RankHierarchy> { new RankHierarchyBuilder().WithName("Officer").Build() };
            var settingsManager = CreateTestSettingsManager();
            var viewModel = new MainWindowViewModel(mockService.Object, ranks, "C:\\GTA V", "Default", settingsManager);

            // Act
            viewModel.StatusMessage = "Test status message";

            // Assert
            viewModel.StatusMessage.Should().Be("Test status message");
        }

        #endregion

        #region SelectedProfile Tests

        [Fact]
        public void SelectedProfile_Set_UpdatesValue()
        {
            // Arrange
            var mockService = new MockServiceBuilder().BuildMock();
            var ranks = new List<RankHierarchy> { new RankHierarchyBuilder().WithName("Officer").Build() };
            var settingsManager = CreateTestSettingsManager();
            var viewModel = new MainWindowViewModel(mockService.Object, ranks, "C:\\GTA V", "Default", settingsManager);

            // Act
            viewModel.SelectedProfile = "CustomProfile";

            // Assert
            viewModel.SelectedProfile.Should().Be("CustomProfile");
        }

        // Note: OnProfileChanged() requires Avalonia dialogs and file I/O, which cannot be unit tested

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_InitializesViewModels()
        {
            // Arrange & Act
            var mockService = new MockServiceBuilder().BuildMock();
            var ranks = new List<RankHierarchy> { new RankHierarchyBuilder().WithName("Officer").Build() };
            var settingsManager = CreateTestSettingsManager();
            var viewModel = new MainWindowViewModel(mockService.Object, ranks, "C:\\GTA V", "Default", settingsManager);

            // Assert
            viewModel.RanksViewModel.Should().NotBeNull();
            viewModel.StationAssignmentsViewModel.Should().NotBeNull();
            viewModel.VehiclesViewModel.Should().NotBeNull();
            viewModel.OutfitsViewModel.Should().NotBeNull();
            viewModel.SettingsViewModel.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_WithNullRanks_InitializesEmptyViewModels()
        {
            // Arrange & Act
            var mockService = new MockServiceBuilder().BuildMock();
            var settingsManager = CreateTestSettingsManager();
            var viewModel = new MainWindowViewModel(mockService.Object, null, "C:\\GTA V", "Default", settingsManager);

            // Assert
            viewModel.RanksViewModel.Should().NotBeNull();
            viewModel.RanksViewModel.RankTreeItems.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_InitializesCommands()
        {
            // Arrange & Act
            var mockService = new MockServiceBuilder().BuildMock();
            var ranks = new List<RankHierarchy> { new RankHierarchyBuilder().WithName("Officer").Build() };
            var settingsManager = CreateTestSettingsManager();
            var viewModel = new MainWindowViewModel(mockService.Object, ranks, "C:\\GTA V", "Default", settingsManager);

            // Assert
            viewModel.GenerateCommand.Should().NotBeNull();
            viewModel.ExitCommand.Should().NotBeNull();
            viewModel.UndoCommand.Should().NotBeNull();
            viewModel.RedoCommand.Should().NotBeNull();
            viewModel.ToggleXmlPreviewCommand.Should().NotBeNull();
            viewModel.ToggleXmlPreviewThemeCommand.Should().NotBeNull();
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Creates a test SettingsManager with a temporary file
        /// </summary>
        private SettingsManager CreateTestSettingsManager()
        {
            var tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"test_settings_{System.Guid.NewGuid()}.ini");
            return new SettingsManager(tempPath);
        }

        #endregion
    }
}
