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
    /// Tests for StationAssignmentsViewModel
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Component", "ViewModels")]
    public class StationAssignmentsViewModelTests
    {
        private readonly Mock<DataLoadingService> _mockDataService;

        public StationAssignmentsViewModelTests()
        {
            _mockDataService = new MockServiceBuilder()
                .WithDefaultAgencies()
                .WithDefaultStations()
                .BuildMock();
        }

        #region Initialization Tests

        [Fact]
        public void Constructor_InitializesCollections()
        {
            // Act
            var viewModel = new StationAssignmentsViewModel(_mockDataService.Object);

            // Assert
            viewModel.RankList.Should().NotBeNull();
            viewModel.CopyFromRankList.Should().NotBeNull();
            viewModel.CopyToRankList.Should().NotBeNull();
            viewModel.AssignedStations.Should().NotBeNull();
            viewModel.AvailableStations.Should().NotBeNull();
            viewModel.AgencyFilters.Should().NotBeNull();
            viewModel.SelectedAssignedStations.Should().NotBeNull();
            viewModel.SelectedAvailableStations.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_InitializesCommands()
        {
            // Act
            var viewModel = new StationAssignmentsViewModel(_mockDataService.Object);

            // Assert
            viewModel.AddStationsCommand.Should().NotBeNull();
            viewModel.AddAllStationsCommand.Should().NotBeNull();
            viewModel.RemoveStationsCommand.Should().NotBeNull();
            viewModel.RemoveAllStationsCommand.Should().NotBeNull();
            viewModel.CopyFromRankCommand.Should().NotBeNull();
            viewModel.CopyStationsCommand.Should().NotBeNull();
            viewModel.CopyStationsToRankCommand.Should().NotBeNull();
            viewModel.ClearFiltersCommand.Should().NotBeNull();
            viewModel.UndoCommand.Should().NotBeNull();
            viewModel.RedoCommand.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_LoadsAgencyFilters()
        {
            // Act
            var viewModel = new StationAssignmentsViewModel(_mockDataService.Object);

            // Assert
            viewModel.AgencyFilters.Should().NotBeEmpty("agencies should be loaded into filters");
        }

        #endregion

        #region Property Tests

        [Fact]
        public void SelectedRank_CanBeSet()
        {
            // Arrange
            var viewModel = new StationAssignmentsViewModel(_mockDataService.Object);
            var rank = new RankHierarchyBuilder().WithName("Officer").Build();

            // Act
            viewModel.SelectedRank = rank;

            // Assert
            viewModel.SelectedRank.Should().Be(rank);
        }

        [Fact]
        public void SelectedRank_RaisesPropertyChanged()
        {
            // Arrange
            var viewModel = new StationAssignmentsViewModel(_mockDataService.Object);
            var rank = new RankHierarchyBuilder().WithName("Officer").Build();
            var eventRaised = false;

            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(viewModel.SelectedRank))
                    eventRaised = true;
            };

            // Act
            viewModel.SelectedRank = rank;

            // Assert
            eventRaised.Should().BeTrue();
        }

        [Fact]
        public void SelectedCopyFromRank_CanBeSet()
        {
            // Arrange
            var viewModel = new StationAssignmentsViewModel(_mockDataService.Object);
            var rank = new RankHierarchyBuilder().WithName("Detective").Build();

            // Act
            viewModel.SelectedCopyFromRank = rank;

            // Assert
            viewModel.SelectedCopyFromRank.Should().Be(rank);
        }

        [Fact]
        public void SelectedCopyToRank_CanBeSet()
        {
            // Arrange
            var viewModel = new StationAssignmentsViewModel(_mockDataService.Object);
            var rank = new RankHierarchyBuilder().WithName("Sergeant").Build();

            // Act
            viewModel.SelectedCopyToRank = rank;

            // Assert
            viewModel.SelectedCopyToRank.Should().Be(rank);
        }

        [Fact]
        public void SearchText_DefaultsToEmpty()
        {
            // Act
            var viewModel = new StationAssignmentsViewModel(_mockDataService.Object);

            // Assert
            viewModel.SearchText.Should().BeEmpty();
        }

        [Fact]
        public void SearchText_CanBeSet()
        {
            // Arrange
            var viewModel = new StationAssignmentsViewModel(_mockDataService.Object);

            // Act
            viewModel.SearchText = "Mission Row";

            // Assert
            viewModel.SearchText.Should().Be("Mission Row");
        }

        [Fact]
        public void RemoveButtonText_DefaultsToRemove()
        {
            // Act
            var viewModel = new StationAssignmentsViewModel(_mockDataService.Object);

            // Assert
            viewModel.RemoveButtonText.Should().Be("Remove");
        }

        [Fact]
        public void RemoveButtonEnabled_DefaultsToFalse()
        {
            // Act
            var viewModel = new StationAssignmentsViewModel(_mockDataService.Object);

            // Assert
            viewModel.RemoveButtonEnabled.Should().BeFalse();
        }

        [Fact]
        public void ShowCopyFromWarning_IsFalseWhenNoRankSelected()
        {
            // Act
            var viewModel = new StationAssignmentsViewModel(_mockDataService.Object);

            // Assert
            viewModel.ShowCopyFromWarning.Should().BeFalse();
        }

        [Fact]
        public void ShowCopyFromWarning_IsTrueWhenRankSelected()
        {
            // Arrange
            var viewModel = new StationAssignmentsViewModel(_mockDataService.Object);
            var rank = new RankHierarchyBuilder().WithName("Officer").Build();

            // Act
            viewModel.SelectedCopyFromRank = rank;

            // Assert
            viewModel.ShowCopyFromWarning.Should().BeTrue();
        }

        [Fact]
        public void ShowCopyToWarning_IsFalseWhenNoRankSelected()
        {
            // Act
            var viewModel = new StationAssignmentsViewModel(_mockDataService.Object);

            // Assert
            viewModel.ShowCopyToWarning.Should().BeFalse();
        }

        [Fact]
        public void ShowCopyToWarning_IsTrueWhenRankSelected()
        {
            // Arrange
            var viewModel = new StationAssignmentsViewModel(_mockDataService.Object);
            var rank = new RankHierarchyBuilder().WithName("Sergeant").Build();

            // Act
            viewModel.SelectedCopyToRank = rank;

            // Assert
            viewModel.ShowCopyToWarning.Should().BeTrue();
        }

        #endregion

        #region Command Tests

        [Fact]
        public void AddStationsCommand_IsCreated()
        {
            // Act
            var viewModel = new StationAssignmentsViewModel(_mockDataService.Object);

            // Assert
            viewModel.AddStationsCommand.Should().NotBeNull();
        }

        [Fact]
        public void AddAllStationsCommand_IsCreated()
        {
            // Act
            var viewModel = new StationAssignmentsViewModel(_mockDataService.Object);

            // Assert
            viewModel.AddAllStationsCommand.Should().NotBeNull();
        }

        [Fact]
        public void RemoveStationsCommand_IsCreated()
        {
            // Act
            var viewModel = new StationAssignmentsViewModel(_mockDataService.Object);

            // Assert
            viewModel.RemoveStationsCommand.Should().NotBeNull();
        }

        [Fact]
        public void RemoveAllStationsCommand_IsCreated()
        {
            // Act
            var viewModel = new StationAssignmentsViewModel(_mockDataService.Object);

            // Assert
            viewModel.RemoveAllStationsCommand.Should().NotBeNull();
        }

        [Fact]
        public void CopyFromRankCommand_IsCreated()
        {
            // Act
            var viewModel = new StationAssignmentsViewModel(_mockDataService.Object);

            // Assert
            viewModel.CopyFromRankCommand.Should().NotBeNull();
        }

        [Fact]
        public void CopyStationsCommand_IsCreated()
        {
            // Act
            var viewModel = new StationAssignmentsViewModel(_mockDataService.Object);

            // Assert
            viewModel.CopyStationsCommand.Should().NotBeNull();
        }

        [Fact]
        public void CopyStationsToRankCommand_IsCreated()
        {
            // Act
            var viewModel = new StationAssignmentsViewModel(_mockDataService.Object);

            // Assert
            viewModel.CopyStationsToRankCommand.Should().NotBeNull();
        }

        [Fact]
        public void ClearFiltersCommand_IsCreated()
        {
            // Act
            var viewModel = new StationAssignmentsViewModel(_mockDataService.Object);

            // Assert
            viewModel.ClearFiltersCommand.Should().NotBeNull();
        }

        [Fact]
        public void UndoCommand_IsCreated()
        {
            // Act
            var viewModel = new StationAssignmentsViewModel(_mockDataService.Object);

            // Assert
            viewModel.UndoCommand.Should().NotBeNull();
        }

        [Fact]
        public void RedoCommand_IsCreated()
        {
            // Act
            var viewModel = new StationAssignmentsViewModel(_mockDataService.Object);

            // Assert
            viewModel.RedoCommand.Should().NotBeNull();
        }

        #endregion
    }
}
