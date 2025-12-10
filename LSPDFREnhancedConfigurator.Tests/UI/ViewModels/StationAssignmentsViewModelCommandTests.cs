using System.Collections.Generic;
using System.Linq;
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
    /// Tests for StationAssignmentsViewModel command execution - Add, Remove, Copy, Undo/Redo
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Component", "ViewModels")]
    public class StationAssignmentsViewModelCommandTests
    {
        #region AddStationsCommand Tests

        [Fact]
        public void AddStationsCommand_CanExecute_WithNoRankSelected_ReturnsFalse()
        {
            // Arrange
            var mockService = CreateMockServiceWithStations();
            var viewModel = new StationAssignmentsViewModel(mockService.Object);
            viewModel.LoadRanks(new List<RankHierarchy>());

            // Act & Assert
            viewModel.AddStationsCommand.CanExecute(null).Should().BeFalse("no rank selected");
        }

        [Fact]
        public void AddStationsCommand_CanExecute_WithRankButNoSelection_ReturnsFalse()
        {
            // Arrange
            var officer = new RankHierarchyBuilder().WithName("Officer").Build();
            var mockService = CreateMockServiceWithStations();
            var viewModel = new StationAssignmentsViewModel(mockService.Object);
            viewModel.LoadRanks(new List<RankHierarchy> { officer });

            // SelectedRank is auto-selected, but SelectedAvailableStations is empty
            // Act & Assert
            viewModel.AddStationsCommand.CanExecute(null).Should().BeFalse("no available stations selected");
        }

        [Fact]
        public void AddStationsCommand_CanExecute_WithRankAndSelection_ReturnsTrue()
        {
            // Arrange
            var officer = new RankHierarchyBuilder().WithName("Officer").Build();
            var mockService = CreateMockServiceWithStations();
            var viewModel = new StationAssignmentsViewModel(mockService.Object);
            viewModel.LoadRanks(new List<RankHierarchy> { officer });

            // Add a station to SelectedAvailableStations
            var station = new Station("Mission Row", "LSPD", "MissionRow");
            viewModel.SelectedAvailableStations.Add(station);

            // Act & Assert
            viewModel.AddStationsCommand.CanExecute(null).Should().BeTrue("rank selected and stations selected");
        }

        #endregion

        #region AddAllStationsCommand Tests

        [Fact]
        public void AddAllStationsCommand_CanExecute_WithNoRank_ReturnsFalse()
        {
            // Arrange
            var mockService = CreateMockServiceWithStations();
            var viewModel = new StationAssignmentsViewModel(mockService.Object);
            viewModel.LoadRanks(new List<RankHierarchy>());

            // Act & Assert
            viewModel.AddAllStationsCommand.CanExecute(null).Should().BeFalse("no rank selected");
        }

        [Fact]
        public void AddAllStationsCommand_CanExecute_WithRankButNoAvailableStations_ReturnsFalse()
        {
            // Arrange - Create mock service with NO stations
            var mockService = new MockServiceBuilder().BuildMock();
            var viewModel = new StationAssignmentsViewModel(mockService.Object);

            var officer = new RankHierarchyBuilder().WithName("Officer").Build();
            viewModel.LoadRanks(new List<RankHierarchy> { officer });

            // Act & Assert
            viewModel.AddAllStationsCommand.CanExecute(null).Should().BeFalse("no available stations");
        }

        [Fact]
        public void AddAllStationsCommand_CanExecute_WithRankAndAvailableStations_ReturnsTrue()
        {
            // Arrange
            var officer = new RankHierarchyBuilder().WithName("Officer").Build();
            var mockService = CreateMockServiceWithStations();
            var viewModel = new StationAssignmentsViewModel(mockService.Object);
            viewModel.LoadRanks(new List<RankHierarchy> { officer });

            // Act & Assert - AvailableStations should be populated after LoadRanks
            viewModel.AddAllStationsCommand.CanExecute(null).Should().BeTrue("rank selected and stations available");
        }

        #endregion

        #region RemoveStationsCommand Tests

        [Fact]
        public void RemoveStationsCommand_CanExecute_WithNoRank_ReturnsFalse()
        {
            // Arrange
            var mockService = CreateMockServiceWithStations();
            var viewModel = new StationAssignmentsViewModel(mockService.Object);
            viewModel.LoadRanks(new List<RankHierarchy>());

            // Act & Assert
            viewModel.RemoveStationsCommand.CanExecute(null).Should().BeFalse("no rank selected");
        }

        [Fact]
        public void RemoveStationsCommand_CanExecute_WithRankButNoSelection_ReturnsFalse()
        {
            // Arrange
            var officer = new RankHierarchyBuilder().WithName("Officer").Build();
            officer.Stations.Add(new StationAssignment("Mission Row", new List<string>(), 1));

            var mockService = CreateMockServiceWithStations();
            var viewModel = new StationAssignmentsViewModel(mockService.Object);
            viewModel.LoadRanks(new List<RankHierarchy> { officer });

            // SelectedAssignedStations is empty
            // Act & Assert
            viewModel.RemoveStationsCommand.CanExecute(null).Should().BeFalse("no assigned stations selected");
        }

        [Fact]
        public void RemoveStationsCommand_CanExecute_WithRankAndSelection_ReturnsTrue()
        {
            // Arrange
            var officer = new RankHierarchyBuilder().WithName("Officer").Build();
            var assignment = new StationAssignment("Mission Row", new List<string>(), 1);
            officer.Stations.Add(assignment);

            var mockService = CreateMockServiceWithStations();
            var viewModel = new StationAssignmentsViewModel(mockService.Object);
            viewModel.LoadRanks(new List<RankHierarchy> { officer });

            // Add to SelectedAssignedStations
            viewModel.SelectedAssignedStations.Add(assignment);

            // Act & Assert
            viewModel.RemoveStationsCommand.CanExecute(null).Should().BeTrue("rank selected and stations selected");
        }

        #endregion

        #region RemoveAllStationsCommand Tests

        [Fact]
        public void RemoveAllStationsCommand_Execute_RemovesAllStations()
        {
            // Arrange
            var officer = new RankHierarchyBuilder().WithName("Officer").Build();
            officer.Stations.Add(new StationAssignment("Mission Row", new List<string>(), 1));
            officer.Stations.Add(new StationAssignment("Davis", new List<string>(), 1));

            var mockService = CreateMockServiceWithStations();
            var viewModel = new StationAssignmentsViewModel(mockService.Object);
            viewModel.LoadRanks(new List<RankHierarchy> { officer });

            // Act
            viewModel.RemoveAllStationsCommand.Execute(null);

            // Assert
            officer.Stations.Should().BeEmpty("all stations should be removed");
        }

        [Fact]
        public void RemoveAllStationsCommand_CanExecute_WithNoRank_ReturnsFalse()
        {
            // Arrange
            var mockService = CreateMockServiceWithStations();
            var viewModel = new StationAssignmentsViewModel(mockService.Object);
            viewModel.LoadRanks(new List<RankHierarchy>());

            // Act & Assert
            viewModel.RemoveAllStationsCommand.CanExecute(null).Should().BeFalse("no rank selected");
        }

        [Fact]
        public void RemoveAllStationsCommand_CanExecute_WithRankButNoStations_ReturnsFalse()
        {
            // Arrange
            var officer = new RankHierarchyBuilder().WithName("Officer").Build();
            var mockService = CreateMockServiceWithStations();
            var viewModel = new StationAssignmentsViewModel(mockService.Object);
            viewModel.LoadRanks(new List<RankHierarchy> { officer });

            // Act & Assert
            viewModel.RemoveAllStationsCommand.CanExecute(null).Should().BeFalse("no stations assigned");
        }

        [Fact]
        public void RemoveAllStationsCommand_CanExecute_WithStations_ReturnsTrue()
        {
            // Arrange
            var officer = new RankHierarchyBuilder().WithName("Officer").Build();
            officer.Stations.Add(new StationAssignment("Mission Row", new List<string>(), 1));

            var mockService = CreateMockServiceWithStations();
            var viewModel = new StationAssignmentsViewModel(mockService.Object);
            viewModel.LoadRanks(new List<RankHierarchy> { officer });

            // Act & Assert
            viewModel.RemoveAllStationsCommand.CanExecute(null).Should().BeTrue("stations exist to remove");
        }

        #endregion

        #region CopyFromRankCommand Tests

        [Fact]
        public void CopyFromRankCommand_CanExecute_WithNoRank_ReturnsFalse()
        {
            // Arrange
            var mockService = CreateMockServiceWithStations();
            var viewModel = new StationAssignmentsViewModel(mockService.Object);
            viewModel.LoadRanks(new List<RankHierarchy>());

            // Act & Assert
            viewModel.CopyFromRankCommand.CanExecute(null).Should().BeFalse("no rank selected");
        }

        [Fact]
        public void CopyFromRankCommand_CanExecute_WithRank_ReturnsTrue()
        {
            // Arrange
            var officer = new RankHierarchyBuilder().WithName("Officer").Build();
            var mockService = CreateMockServiceWithStations();
            var viewModel = new StationAssignmentsViewModel(mockService.Object);
            viewModel.LoadRanks(new List<RankHierarchy> { officer });

            // Act & Assert
            viewModel.CopyFromRankCommand.CanExecute(null).Should().BeTrue("rank selected");
        }

        // Note: CopyFromRankCommand.Execute() shows an Avalonia dialog
        // which cannot be tested in unit tests. We only test CanExecute logic here.

        #endregion

        #region CopyStationsCommand Tests

        [Fact]
        public void CopyStationsCommand_CanExecute_WithNoSelection_ReturnsFalse()
        {
            // Arrange
            var mockService = CreateMockServiceWithStations();
            var viewModel = new StationAssignmentsViewModel(mockService.Object);
            viewModel.LoadRanks(new List<RankHierarchy>());

            // Act & Assert
            viewModel.CopyStationsCommand.CanExecute(null).Should().BeFalse("no ranks selected");
        }

        [Fact]
        public void CopyStationsCommand_CanExecute_WithBothRanksSelected_ReturnsTrue()
        {
            // Arrange
            var officer = new RankHierarchyBuilder().WithName("Officer").Build();
            var detective = new RankHierarchyBuilder().WithName("Detective").Build();

            var mockService = CreateMockServiceWithStations();
            var viewModel = new StationAssignmentsViewModel(mockService.Object);
            viewModel.LoadRanks(new List<RankHierarchy> { officer, detective });

            // Set both SelectedRank and SelectedCopyFromRank
            viewModel.SelectedRank = officer;
            viewModel.SelectedCopyFromRank = detective;

            // Act & Assert
            viewModel.CopyStationsCommand.CanExecute(null).Should().BeTrue("both ranks selected");
        }

        #endregion

        #region CopyStationsToRankCommand Tests

        [Fact]
        public void CopyStationsToRankCommand_CanExecute_WithNoSelection_ReturnsFalse()
        {
            // Arrange
            var mockService = CreateMockServiceWithStations();
            var viewModel = new StationAssignmentsViewModel(mockService.Object);
            viewModel.LoadRanks(new List<RankHierarchy>());

            // Act & Assert
            viewModel.CopyStationsToRankCommand.CanExecute(null).Should().BeFalse("no ranks selected");
        }

        [Fact]
        public void CopyStationsToRankCommand_CanExecute_WithSameRank_ReturnsFalse()
        {
            // Arrange
            var officer = new RankHierarchyBuilder().WithName("Officer").Build();

            var mockService = CreateMockServiceWithStations();
            var viewModel = new StationAssignmentsViewModel(mockService.Object);
            viewModel.LoadRanks(new List<RankHierarchy> { officer });

            // Set both to same rank
            viewModel.SelectedRank = officer;
            viewModel.SelectedCopyToRank = officer;

            // Act & Assert
            viewModel.CopyStationsToRankCommand.CanExecute(null).Should().BeFalse("cannot copy to same rank");
        }

        [Fact]
        public void CopyStationsToRankCommand_CanExecute_WithDifferentRanks_ReturnsTrue()
        {
            // Arrange
            var officer = new RankHierarchyBuilder().WithName("Officer").Build();
            var detective = new RankHierarchyBuilder().WithName("Detective").Build();

            var mockService = CreateMockServiceWithStations();
            var viewModel = new StationAssignmentsViewModel(mockService.Object);
            viewModel.LoadRanks(new List<RankHierarchy> { officer, detective });

            // Set different ranks
            viewModel.SelectedRank = officer;
            viewModel.SelectedCopyToRank = detective;

            // Act & Assert
            viewModel.CopyStationsToRankCommand.CanExecute(null).Should().BeTrue("different ranks selected");
        }

        #endregion

        #region ClearFiltersCommand Tests

        [Fact]
        public void ClearFiltersCommand_Execute_ClearsSearchText()
        {
            // Arrange
            var officer = new RankHierarchyBuilder().WithName("Officer").Build();
            var mockService = CreateMockServiceWithStations();
            var viewModel = new StationAssignmentsViewModel(mockService.Object);
            viewModel.LoadRanks(new List<RankHierarchy> { officer });

            viewModel.SearchText = "Mission";

            // Act
            viewModel.ClearFiltersCommand.Execute(null);

            // Assert
            viewModel.SearchText.Should().BeEmpty("search text should be cleared");
        }

        [Fact]
        public void ClearFiltersCommand_Execute_UnchecksAllAgencyFilters()
        {
            // Arrange
            var officer = new RankHierarchyBuilder().WithName("Officer").Build();
            var mockService = CreateMockServiceWithStations();
            var viewModel = new StationAssignmentsViewModel(mockService.Object);
            viewModel.LoadRanks(new List<RankHierarchy> { officer });

            // Check some filters
            if (viewModel.AgencyFilters.Count > 0)
            {
                viewModel.AgencyFilters[0].IsChecked = true;
            }

            // Act
            viewModel.ClearFiltersCommand.Execute(null);

            // Assert
            viewModel.AgencyFilters.Should().OnlyContain(f => f.IsChecked == false, "all filters should be unchecked");
        }

        #endregion

        #region Undo/Redo Command Tests

        [Fact]
        public void UndoCommand_Execute_UndoesRemoveAllStations()
        {
            // Arrange
            var officer = new RankHierarchyBuilder().WithName("Officer").Build();
            officer.Stations.Add(new StationAssignment("Mission Row", new List<string>(), 1));
            officer.Stations.Add(new StationAssignment("Davis", new List<string>(), 1));

            var mockService = CreateMockServiceWithStations();
            var viewModel = new StationAssignmentsViewModel(mockService.Object);
            viewModel.LoadRanks(new List<RankHierarchy> { officer });

            // Remove all stations
            viewModel.RemoveAllStationsCommand.Execute(null);
            officer.Stations.Should().BeEmpty();

            // Act - Undo
            viewModel.UndoCommand.Execute(null);

            // Assert
            officer.Stations.Should().HaveCount(2, "undo should restore stations");
        }

        [Fact]
        public void RedoCommand_Execute_RedoesRemoveAllStations()
        {
            // Arrange
            var officer = new RankHierarchyBuilder().WithName("Officer").Build();
            officer.Stations.Add(new StationAssignment("Mission Row", new List<string>(), 1));

            var mockService = CreateMockServiceWithStations();
            var viewModel = new StationAssignmentsViewModel(mockService.Object);
            viewModel.LoadRanks(new List<RankHierarchy> { officer });

            // Remove all, then undo
            viewModel.RemoveAllStationsCommand.Execute(null);
            viewModel.UndoCommand.Execute(null);
            officer.Stations.Should().HaveCount(1);

            // Act - Redo
            viewModel.RedoCommand.Execute(null);

            // Assert
            officer.Stations.Should().BeEmpty("redo should remove stations again");
        }

        [Fact]
        public void UndoCommand_CanExecute_WithNoHistory_ReturnsFalse()
        {
            // Arrange
            var mockService = CreateMockServiceWithStations();
            var viewModel = new StationAssignmentsViewModel(mockService.Object);
            viewModel.LoadRanks(new List<RankHierarchy>());

            // Act & Assert
            viewModel.UndoCommand.CanExecute(null).Should().BeFalse("no undo history");
        }

        [Fact]
        public void UndoCommand_CanExecute_AfterRemoveAll_ReturnsTrue()
        {
            // Arrange
            var officer = new RankHierarchyBuilder().WithName("Officer").Build();
            officer.Stations.Add(new StationAssignment("Mission Row", new List<string>(), 1));

            var mockService = CreateMockServiceWithStations();
            var viewModel = new StationAssignmentsViewModel(mockService.Object);
            viewModel.LoadRanks(new List<RankHierarchy> { officer });

            viewModel.RemoveAllStationsCommand.Execute(null);

            // Act & Assert
            viewModel.UndoCommand.CanExecute(null).Should().BeTrue("undo available after RemoveAll");
        }

        [Fact]
        public void RedoCommand_CanExecute_WithNoRedoHistory_ReturnsFalse()
        {
            // Arrange
            var mockService = CreateMockServiceWithStations();
            var viewModel = new StationAssignmentsViewModel(mockService.Object);
            viewModel.LoadRanks(new List<RankHierarchy>());

            // Act & Assert
            viewModel.RedoCommand.CanExecute(null).Should().BeFalse("no redo history");
        }

        [Fact]
        public void RedoCommand_CanExecute_AfterUndo_ReturnsTrue()
        {
            // Arrange
            var officer = new RankHierarchyBuilder().WithName("Officer").Build();
            officer.Stations.Add(new StationAssignment("Mission Row", new List<string>(), 1));

            var mockService = CreateMockServiceWithStations();
            var viewModel = new StationAssignmentsViewModel(mockService.Object);
            viewModel.LoadRanks(new List<RankHierarchy> { officer });

            viewModel.RemoveAllStationsCommand.Execute(null);
            viewModel.UndoCommand.Execute(null);

            // Act & Assert
            viewModel.RedoCommand.CanExecute(null).Should().BeTrue("redo available after undo");
        }

        #endregion

        #region SelectedRank Tests

        [Fact]
        public void SelectedRank_Set_UpdatesCommandStates()
        {
            // Arrange
            var officer = new RankHierarchyBuilder().WithName("Officer").Build();
            officer.Stations.Add(new StationAssignment("Mission Row", new List<string>(), 1));

            var detective = new RankHierarchyBuilder().WithName("Detective").Build();

            var mockService = CreateMockServiceWithStations();
            var viewModel = new StationAssignmentsViewModel(mockService.Object);
            viewModel.LoadRanks(new List<RankHierarchy> { officer, detective });

            // Initially officer is selected (first rank)
            viewModel.RemoveAllStationsCommand.CanExecute(null).Should().BeTrue("officer has stations");

            // Act - Select detective (no stations)
            viewModel.SelectedRank = detective;

            // Assert
            viewModel.RemoveAllStationsCommand.CanExecute(null).Should().BeFalse("detective has no stations");
        }

        [Fact]
        public void LoadRanks_WithMultipleRanks_SelectsFirstRank()
        {
            // Arrange & Act
            var officer = new RankHierarchyBuilder().WithName("Officer").Build();
            var detective = new RankHierarchyBuilder().WithName("Detective").Build();

            var mockService = CreateMockServiceWithStations();
            var viewModel = new StationAssignmentsViewModel(mockService.Object);
            viewModel.LoadRanks(new List<RankHierarchy> { officer, detective });

            // Assert
            viewModel.SelectedRank.Should().NotBeNull("first rank should be auto-selected");
            viewModel.SelectedRank!.Name.Should().Be("Officer");
        }

        [Fact]
        public void LoadRanks_WithNoRanks_HasNoSelection()
        {
            // Arrange & Act
            var mockService = CreateMockServiceWithStations();
            var viewModel = new StationAssignmentsViewModel(mockService.Object);
            viewModel.LoadRanks(new List<RankHierarchy>());

            // Assert
            viewModel.SelectedRank.Should().BeNull("no ranks available");
        }

        #endregion

        #region RankList Population Tests

        [Fact]
        public void LoadRanks_PopulatesRankList()
        {
            // Arrange & Act
            var officer = new RankHierarchyBuilder().WithName("Officer").Build();
            var detective = new RankHierarchyBuilder().WithName("Detective").Build();
            var sergeant = new RankHierarchyBuilder().WithName("Sergeant").Build();

            var mockService = CreateMockServiceWithStations();
            var viewModel = new StationAssignmentsViewModel(mockService.Object);
            viewModel.LoadRanks(new List<RankHierarchy> { officer, detective, sergeant });

            // Assert
            viewModel.RankList.Should().HaveCount(3);
            viewModel.RankList[0].Name.Should().Be("Officer");
            viewModel.RankList[1].Name.Should().Be("Detective");
            viewModel.RankList[2].Name.Should().Be("Sergeant");
        }

        [Fact]
        public void LoadRanks_WithNullOrEmpty_HasEmptyRankList()
        {
            // Arrange & Act
            var mockService = CreateMockServiceWithStations();
            var viewModel = new StationAssignmentsViewModel(mockService.Object);
            viewModel.LoadRanks(new List<RankHierarchy>());

            // Assert
            viewModel.RankList.Should().BeEmpty();
        }

        #endregion

        #region RemoveButton State Tests

        [Fact]
        public void RemoveButtonText_WithNoSelection_ShowsRemove()
        {
            // Arrange
            var officer = new RankHierarchyBuilder().WithName("Officer").Build();
            officer.Stations.Add(new StationAssignment("Mission Row", new List<string>(), 1));

            var mockService = CreateMockServiceWithStations();
            var viewModel = new StationAssignmentsViewModel(mockService.Object);
            viewModel.LoadRanks(new List<RankHierarchy> { officer });

            // Act & Assert
            viewModel.RemoveButtonText.Should().Be("Remove");
            viewModel.RemoveButtonEnabled.Should().BeFalse("no stations selected");
        }

        [Fact]
        public void RemoveButtonText_WithSingleSelection_ShowsRemove()
        {
            // Arrange
            var officer = new RankHierarchyBuilder().WithName("Officer").Build();
            var assignment = new StationAssignment("Mission Row", new List<string>(), 1);
            officer.Stations.Add(assignment);

            var mockService = CreateMockServiceWithStations();
            var viewModel = new StationAssignmentsViewModel(mockService.Object);
            viewModel.LoadRanks(new List<RankHierarchy> { officer });

            // Add to selection
            viewModel.SelectedAssignedStations.Add(assignment);

            // Act & Assert
            viewModel.RemoveButtonText.Should().Be("Remove");
            viewModel.RemoveButtonEnabled.Should().BeTrue("one station selected");
        }

        [Fact]
        public void RemoveButtonText_WithMultipleSelection_ShowsCount()
        {
            // Arrange
            var officer = new RankHierarchyBuilder().WithName("Officer").Build();
            var assignment1 = new StationAssignment("Mission Row", new List<string>(), 1);
            var assignment2 = new StationAssignment("Davis", new List<string>(), 1);
            officer.Stations.Add(assignment1);
            officer.Stations.Add(assignment2);

            var mockService = CreateMockServiceWithStations();
            var viewModel = new StationAssignmentsViewModel(mockService.Object);
            viewModel.LoadRanks(new List<RankHierarchy> { officer });

            // Add both to selection
            viewModel.SelectedAssignedStations.Add(assignment1);
            viewModel.SelectedAssignedStations.Add(assignment2);

            // Act & Assert
            viewModel.RemoveButtonText.Should().Be("Remove selected (2)");
            viewModel.RemoveButtonEnabled.Should().BeTrue("multiple stations selected");
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Creates a mock DataLoadingService with sample stations and agencies
        /// </summary>
        private Mock<DataLoadingService> CreateMockServiceWithStations()
        {
            var mockService = new MockServiceBuilder().BuildMock();

            // Add sample stations
            var stations = new List<Station>
            {
                new Station("Mission Row Police Station", "LSPD", "MissionRow"),
                new Station("Davis Police Station", "LSPD", "Davis"),
                new Station("Sandy Shores Sheriff Station", "LSSD", "SandyShores")
            };
            mockService.Setup(m => m.Stations).Returns(stations);

            // Add sample agencies
            var agencies = new List<Agency>
            {
                new Agency("Los Santos Police Department", "LSPD", "lspd"),
                new Agency("Los Santos Sheriff Department", "LSSD", "lssd")
            };
            mockService.Setup(m => m.Agencies).Returns(agencies);

            return mockService;
        }

        #endregion
    }
}
